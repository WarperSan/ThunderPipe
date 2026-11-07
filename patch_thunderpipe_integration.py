"""Apply the Issue #124 automation files to a local ThunderPipe checkout.

Standard library only. Overwrites the listed files in place; no backups or preflight checks.
Run: python patch_thunderpipe_integration.py D:\\ThunderPipe
"""
from pathlib import Path
import sys

root = Path(sys.argv[1] if len(sys.argv) > 1 else r"D:\ThunderPipe")

files = {
".github/workflows/test-integration.yml": r'''name: Thunderstore release compatibility integration tests

on:
  schedule:
    - cron: '17 3 * * *'
  push:
    branches:
      - feature/integration-tests
  workflow_dispatch:
    inputs:
      force:
        description: 'Retest the latest stable release even if it was already tested'
        type: boolean
        default: true
        required: true

permissions:
  contents: read

concurrency:
  group: thunderstore-release-compatibility
  cancel-in-progress: false

jobs:
  check-release:
    runs-on: ubuntu-latest
    timeout-minutes: 5
    permissions:
      contents: write
    outputs:
      should_test: ${{ steps.check.outputs.should_test }}
      release_tag: ${{ steps.check.outputs.release_tag }}
    steps:
      - name: Check out ThunderPipe
        uses: actions/checkout@v4
        with:
          persist-credentials: false

      - name: Discover latest stable Thunderstore release
        id: check
        env:
          GH_TOKEN: ${{ github.token }}
          REPOSITORY: ${{ github.repository }}
          STATE_BRANCH: thunderstore-integration-state
          FORCE_TEST: ${{ (github.event_name == 'workflow_dispatch' && inputs.force) || (github.event_name == 'push' && contains(github.event.head_commit.message, '[retest-integration]')) }}
        run: python3 ThunderPipe.Core.IntegrationTests/scripts/check_thunderstore_release.py discover

  integration:
    needs: check-release
    if: needs.check-release.outputs.should_test == 'true'
    runs-on: ubuntu-latest
    timeout-minutes: 120
    permissions:
      contents: write
    env:
      THUNDERSTORE_DIR: ${{ github.workspace }}/thunderstore-src
      INTEGRATION_ARTIFACTS: ${{ github.workspace }}/ThunderPipe.Core.IntegrationTests/artifacts
      THUNDERSTORE_RELEASE: ${{ needs.check-release.outputs.release_tag }}
    steps:
      - name: Check out current ThunderPipe source
        uses: actions/checkout@v4
        with:
          persist-credentials: false

      - name: Check out the exact Thunderstore release
        uses: actions/checkout@v4
        with:
          repository: thunderstore-io/Thunderstore
          ref: ${{ needs.check-release.outputs.release_tag }}
          path: thunderstore-src
          submodules: recursive
          persist-credentials: false

      - name: Prepare isolated Docker environment and fixture
        shell: bash
        run: |
          set -euo pipefail
          mkdir -p "$INTEGRATION_ARTIFACTS"
          echo environment_failure > "$INTEGRATION_ARTIFACTS/result-category"
          bash ThunderPipe.Core.IntegrationTests/scripts/run-integration-linux.sh setup \
            2>&1 | tee "$INTEGRATION_ARTIFACTS/setup.log"

      - name: Run only ThunderPipe.Core.IntegrationTests
        shell: bash
        run: |
          set -euo pipefail
          bash ThunderPipe.Core.IntegrationTests/scripts/run-integration-linux.sh test \
            2>&1 | tee "$INTEGRATION_ARTIFACTS/test.log"

      - name: Collect Docker and server-side evidence
        if: always()
        shell: bash
        run: bash ThunderPipe.Core.IntegrationTests/scripts/run-integration-linux.sh collect || true

      - name: Persist terminal outcome (environment failures remain retryable)
        if: always()
        env:
          GH_TOKEN: ${{ github.token }}
          REPOSITORY: ${{ github.repository }}
          STATE_BRANCH: thunderstore-integration-state
        shell: bash
        run: |
          set -euo pipefail
          CATEGORY=environment_failure
          if [[ -f "$INTEGRATION_ARTIFACTS/result-category" ]]; then
            CATEGORY="$(cat "$INTEGRATION_ARTIFACTS/result-category")"
          fi
          python3 ThunderPipe.Core.IntegrationTests/scripts/check_thunderstore_release.py \
            record "$THUNDERSTORE_RELEASE" "$CATEGORY"

      - name: Upload diagnostic artifacts
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: thunderstore-${{ needs.check-release.outputs.release_tag }}-integration-${{ github.run_attempt }}
          if-no-files-found: warn
          retention-days: 14
          path: |
            ThunderPipe.Core.IntegrationTests/artifacts/*.trx
            ThunderPipe.Core.IntegrationTests/artifacts/client-http.log
            ThunderPipe.Core.IntegrationTests/artifacts/docker-server.log
            ThunderPipe.Core.IntegrationTests/artifacts/server-state.json
            ThunderPipe.Core.IntegrationTests/artifacts/setup.log
            ThunderPipe.Core.IntegrationTests/artifacts/test.log
            ThunderPipe.Core.IntegrationTests/artifacts/result-category

      - name: Stop and remove local Docker test resources
        if: always()
        shell: bash
        run: bash ThunderPipe.Core.IntegrationTests/scripts/run-integration-linux.sh cleanup || true
''',

"ThunderPipe.Core.IntegrationTests/scripts/check_thunderstore_release.py": r'''#!/usr/bin/env python3
"""Discover stable Thunderstore releases and persist tested outcomes in a state branch.

Only successful tests and actual test-assertion failures are terminal.
Infrastructure/setup failures remain retryable on the next scheduled check.
"""
import base64
import json
import os
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timezone

API = "https://api.github.com"
UPSTREAM = "thunderstore-io/Thunderstore"
STATE_FILE = "thunderstore-release-state.json"


class StateConflict(RuntimeError):
    pass


def quoted(value):
    return urllib.parse.quote(value, safe="")


def request(method, path, data=None):
    url = API + path
    headers = {
        "Accept": "application/vnd.github+json",
        "User-Agent": "ThunderPipe-integration-check",
        "X-GitHub-Api-Version": "2022-11-28",
        "Authorization": "Bearer " + os.environ["GH_TOKEN"],
    }
    payload = json.dumps(data).encode("utf-8") if data is not None else None
    if payload is not None:
        headers["Content-Type"] = "application/json"
    req = urllib.request.Request(url, data=payload, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        detail = error.read().decode(errors="replace")[:500]
        if error.code == 404:
            return None
        if error.code in (409, 422):
            raise StateConflict(f"HTTP {error.code}: {detail}") from error
        raise RuntimeError(f"GitHub API {method} {path}: HTTP {error.code}: {detail}") from error


def ensure_branch(repo, branch):
    path = f"/repos/{repo}/git/ref/heads/{quoted(branch)}"
    if request("GET", path) is not None:
        return
    info = request("GET", f"/repos/{repo}")
    base = request("GET", f"/repos/{repo}/git/ref/heads/{quoted(info['default_branch'])}")
    try:
        request("POST", f"/repos/{repo}/git/refs", {
            "ref": f"refs/heads/{branch}", "sha": base["object"]["sha"]
        })
    except StateConflict:
        if request("GET", path) is None:
            raise


def read_state(repo, branch):
    path = f"/repos/{repo}/contents/{STATE_FILE}?ref={quoted(branch)}"
    file = request("GET", path)
    if file is None:
        return {}, None
    return json.loads(base64.b64decode(file["content"]).decode("utf-8")), file["sha"]


def mutate_state(repo, branch, message, transform):
    # GitHub's contents API compares blob SHA on PUT. Re-read and merge if a
    # concurrent run changes the file before our write is accepted.
    for attempt in range(6):
        state, sha = read_state(repo, branch)
        changed = transform(state)
        if not changed:
            return state
        content = json.dumps(state, indent=2, ensure_ascii=False, sort_keys=True) + "\n"
        payload = {
            "message": message,
            "branch": branch,
            "content": base64.b64encode(content.encode("utf-8")).decode("ascii"),
        }
        if sha:
            payload["sha"] = sha
        try:
            request("PUT", f"/repos/{repo}/contents/{STATE_FILE}", payload)
            return state
        except StateConflict:
            if attempt == 5:
                raise
            time.sleep(min(2 ** attempt, 8))
    raise RuntimeError("State could not be updated after concurrent changes")


def output(key, value):
    text = str(value)
    print(f"{key}={text}")
    if os.getenv("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as stream:
            stream.write(f"{key}={text}\n")


def now():
    return datetime.now(timezone.utc).isoformat()


def discover(repo, branch):
    release = request("GET", f"/repos/{UPSTREAM}/releases/latest")
    if not release or release.get("draft") or release.get("prerelease"):
        raise RuntimeError("No usable latest stable Thunderstore release")
    tag = release["tag_name"]
    force = os.getenv("FORCE_TEST", "false").lower() == "true"
    decision = {"should_test": False, "reason": "already tested"}
    def mark(value):
        # Re-evaluate against the latest state on every compare-and-swap retry.
        terminal = (value.get("tested_releases") or {}).get(tag, {})
        should_test = force or terminal.get("status") not in ("success", "test_failure")
        reason = ("manual retest" if force else
                  "already tested" if not should_test else "new or retryable release")
        decision.update(should_test=should_test, reason=reason)
        if not should_test:
            return False
        value["last_checked_release"] = tag
        value["pending_test_release"] = tag
        value["last_check_result"] = {
            "checked_at": now(), "release": tag,
            "requires_integration_tests": True,
            "integration_tests_executed": False, "reason": reason,
        }
        return True
    mutate_state(repo, branch, "chore: schedule Thunderstore integration run", mark)
    output("release_tag", tag)
    output("should_test", str(decision["should_test"]).lower())
    print(decision["reason"])


def record(repo, branch, tag, status):
    if status not in ("success", "test_failure", "environment_failure"):
        raise RuntimeError(f"Unknown test outcome {status!r}")
    result = {
        "release": tag,
        "status": status,
        "recorded_at": now(),
        "run_url": (f"https://github.com/{repo}/actions/runs/"
                    f"{os.environ.get('GITHUB_RUN_ID', 'unknown')}"),
        "thunderpipe_commit": os.environ.get("GITHUB_SHA"),
    }
    def mark(value):
        value["last_test_result"] = result
        value["last_check_result"] = {
            "checked_at": result["recorded_at"],
            "release": tag,
            "requires_integration_tests": True,
            "integration_tests_executed": status != "environment_failure",
            "reason": status,
        }
        if status == "environment_failure":
            value["pending_test_release"] = tag
            value["last_environment_failure"] = result
        else:
            value["pending_test_release"] = None
            value.setdefault("tested_releases", {})[tag] = result
            if status == "success":
                value["last_successful_test_release"] = tag
        return True
    mutate_state(repo, branch, f"chore: record Thunderstore integration {status}", mark)
    print(json.dumps(result, indent=2))


def main():
    repo = os.environ["REPOSITORY"]
    branch = os.getenv("STATE_BRANCH", "thunderstore-integration-state")
    command = sys.argv[1] if len(sys.argv) > 1 else "discover"
    ensure_branch(repo, branch)
    if command == "discover":
        discover(repo, branch)
    elif command == "record" and len(sys.argv) == 4:
        record(repo, branch, sys.argv[2], sys.argv[3])
    else:
        raise SystemExit("Usage: check_thunderstore_release.py discover | record TAG STATUS")


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(f"ERROR: {error}", file=sys.stderr)
        sys.exit(1)
''',

"ThunderPipe.Core.IntegrationTests/scripts/run-integration-linux.sh": r'''#!/usr/bin/env bash
# Linux GitHub Actions adapter for the existing Django fixture and C# HTTP tests.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../" && pwd)"
TS_DIR="${THUNDERSTORE_DIR:-$ROOT/thunderstore-src}"
ART_DIR="${INTEGRATION_ARTIFACTS:-$ROOT/ThunderPipe.Core.IntegrationTests/artifacts}"
PROJECT="${THUNDERSTORE_COMPOSE_PROJECT:-thunderpipe-integration}"
NETWORK="thunderstore_default"  # Defined explicitly by Thunderstore's Compose file.
IMAGE="thunderpipe-integration:${GITHUB_RUN_ID:-local}"
OVERRIDE="$ROOT/ThunderPipe.Core.IntegrationTests/docker-compose.integration.yml"

compose() {
  docker compose --project-directory "$TS_DIR" \
    -f "$TS_DIR/docker-compose.yml" -f "$OVERRIDE" -p "$PROJECT" "$@"
}

setup() {
  mkdir -p "$ART_DIR"
  # These are the release's own local-development settings, not production data.
  cp "$TS_DIR/.env.template" "$TS_DIR/.env"

  # Build ThunderPipe before creating the token fixture. The .dockerignore
  # also prevents fixture/token material from entering the Docker build context.
  docker build -f "$ROOT/ThunderPipe.Core.IntegrationTests/Dockerfile" \
    -t "$IMAGE" "$ROOT"

  compose up -d --build db dbpool redis rabbitmq minio builder django nginx django-worker

  local ready=0
  for i in $(seq 1 45); do
    if compose exec -T django python manage.py shell -c \
       'from django.db import connection; connection.ensure_connection()' >/dev/null 2>&1; then
      ready=1
      break
    fi
    sleep 10
  done
  if [[ "$ready" -ne 1 ]]; then
    echo 'Django database was not ready before the deadline' >&2
    return 1
  fi

  compose exec -T django python manage.py setup_dev_env --skip-test-data
  compose exec -T django python manage.py shell -c \
    "exec(__import__('sys').stdin.read())" \
    < "$ROOT/ThunderPipe.Core.IntegrationTests/scripts/seed_thunderstore.py" \
    > "$ART_DIR/django-seed-raw.log"
  python3 "$ROOT/ThunderPipe.Core.IntegrationTests/scripts/extract_fixture_json.py" \
    "$ART_DIR/django-seed-raw.log" "$ART_DIR/thunderpipe-fixture.json"

  # Check the actual container-to-container DNS names used by the HTTP tests.
  docker run --rm --network "$NETWORK" --entrypoint getent "$IMAGE" \
    hosts thunderstore.localhost minio.localhost

  ready=0
  for i in $(seq 1 30); do
    if curl --fail --silent --max-time 5 \
      -H 'Host: thunderstore.localhost' \
      'http://127.0.0.1/api/experimental/community/' >/dev/null; then
      ready=1
      break
    fi
    sleep 5
  done
  if [[ "$ready" -ne 1 ]]; then
    echo 'HTTP API was not ready before the deadline' >&2
    return 1
  fi

  # Publishing uses real S3 multipart uploads and an asynchronous Celery job.
  # Both must be functional before a failed assertion can indicate incompatibility.
  ready=0
  for i in $(seq 1 30); do
    if compose exec -T django python -c \
       "import boto3; s=boto3.client('s3',endpoint_url='http://minio:9000',aws_access_key_id='thunderstore',aws_secret_access_key='thunderstore',region_name='us-east-1'); s.head_bucket(Bucket='thunderstore')" \
       >/dev/null 2>&1; then
      ready=1
      break
    fi
    sleep 5
  done
  if [[ "$ready" -ne 1 ]]; then
    echo 'S3 test bucket was not ready before the deadline' >&2
    return 1
  fi

  ready=0
  for i in $(seq 1 30); do
    if compose exec -T django celery -A thunderstore.core inspect ping --timeout=3 \
       2>/dev/null | grep -q 'pong'; then
      ready=1
      break
    fi
    sleep 5
  done
  if [[ "$ready" -ne 1 ]]; then
    echo 'Celery worker was not ready before the deadline' >&2
    return 1
  fi
}

test_api() {
  # The result marker remains retryable until a genuine TRX was produced.
  echo environment_failure > "$ART_DIR/result-category"
  local code=0
  docker run --rm --init --network "$NETWORK" \
    --mount "type=bind,src=$ART_DIR,dst=/results" \
    -e THUNDERPIPE_BASE_URL=http://thunderstore.localhost/ \
    -e THUNDERPIPE_FIXTURE_PATH=/results/thunderpipe-fixture.json \
    -e THUNDERPIPE_HTTP_TRACE=/results/client-http.log \
    --entrypoint dotnet "$IMAGE" test \
    --project ThunderPipe.Core.IntegrationTests/ThunderPipe.Core.IntegrationTests.csproj \
    --configuration Release --no-build --no-restore \
    --results-directory /results -- \
    --report-xunit-trx --report-xunit-trx-filename integration.trx || code=$?

  local classification
  classification="$(python3 - "$ART_DIR/integration.trx" "$code" <<'PY'
import sys
import xml.etree.ElementTree as ET
from pathlib import Path
path = Path(sys.argv[1])
code = int(sys.argv[2])
if not path.is_file():
    print('environment_failure')
    sys.exit(0)
try:
    xml = ET.parse(path).getroot()
    counters = next(node for node in xml.iter() if node.tag.endswith('Counters'))
    total = int(counters.attrib.get('total', '0'))
    executed = int(counters.attrib.get('executed', '0'))
    failed = int(counters.attrib.get('failed', '0'))
    if total < 1 or executed < 1:
        print('environment_failure')
    elif failed > 0:
        print('test_failure')
    elif code:
        print('environment_failure')
    else:
        print('success')
except (OSError, ValueError, StopIteration, ET.ParseError):
    print('environment_failure')
PY
)"
  echo "$classification" > "$ART_DIR/result-category"
  echo "Integration result: $classification (test process exit code: $code)"
  [[ "$classification" == 'success' ]]
}

collect() {
  mkdir -p "$ART_DIR"
  compose logs --no-color --timestamps \
    db dbpool redis rabbitmq minio django django-worker nginx builder \
    > "$ART_DIR/docker-server.log" 2>&1 || true
  compose exec -T django python manage.py shell -c \
    "exec(__import__('sys').stdin.read())" \
    < "$ROOT/ThunderPipe.Core.IntegrationTests/scripts/verify_server_state.py" \
    > "$ART_DIR/server-state.json" 2>&1 || true
}

cleanup() {
  compose down -v --remove-orphans
}

case "${1:-}" in
  setup) setup ;;
  test) test_api ;;
  collect) collect ;;
  cleanup) cleanup ;;
  *) echo 'Usage: run-integration-linux.sh {setup|test|collect|cleanup}' >&2; exit 2 ;;
esac
''',

"ThunderPipe.Core.IntegrationTests/docker-compose.integration.yml": r'''# Compose overrides used only by the isolated GitHub Actions integration environment.
# Keep the pinned Thunderstore release's original Dockerfile/services/dependencies.
services:
  nginx:
    networks:
      default:
        aliases:
          - thunderstore.localhost
  minio:
    networks:
      default:
        aliases:
          - minio.localhost
  django:
    environment:
      # The test client only permits .localhost URLs, including S3 presigned URLs.
      USERMEDIA_S3_SIGNING_ENDPOINT_URL: http://minio.localhost:9000/
      AWS_S3_CUSTOM_DOMAIN: minio.localhost:9000/thunderstore
      CACHE_S3_CUSTOM_DOMAIN: minio.localhost:9000/thunderstore
  django-worker:
    environment:
      USERMEDIA_S3_SIGNING_ENDPOINT_URL: http://minio.localhost:9000/
      AWS_S3_CUSTOM_DOMAIN: minio.localhost:9000/thunderstore
      CACHE_S3_CUSTOM_DOMAIN: minio.localhost:9000/thunderstore
''',

"ThunderPipe.Core.IntegrationTests/Dockerfile": r'''# The repo selects .NET 10's MTP test driver while the test target is net8.0.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS net8-runtime
FROM mcr.microsoft.com/dotnet/sdk:10.0
COPY --from=net8-runtime /usr/share/dotnet/shared/ /usr/share/dotnet/shared/
WORKDIR /src
COPY . .
RUN dotnet restore ThunderPipe.Core.IntegrationTests/ThunderPipe.Core.IntegrationTests.csproj
RUN dotnet build ThunderPipe.Core.IntegrationTests/ThunderPipe.Core.IntegrationTests.csproj \
    --configuration Release --no-restore
''',

".dockerignore": r'''**/bin/
**/obj/
.git/
thunderstore-src/
**/artifacts/
**/thunderpipe-fixture.json
**/django-seed-raw.log
**/.env
''',

"global.json": r'''{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",
    "allowPrerelease": false
  },
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
''',
}

for name, content in files.items():
    path = root / name
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content, encoding="utf-8", newline="\n")
    print(name)

print(f"Patched {len(files)} files in {root}")
