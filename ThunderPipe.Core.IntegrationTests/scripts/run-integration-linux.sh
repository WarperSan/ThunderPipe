#!/usr/bin/env bash
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

  # The historical release's python-packages gitlink is inaccessible. Its
  # plugins are optional (BUILD_INSTALL_EXTRAS is empty for this CI run), but
  # its Dockerfile still COPYs this directory.
  mkdir -p "$TS_DIR/python-packages"

  # Apply MinIO fixes to the disposable upstream checkout, never to the
  # upstream Git repository or ThunderPipe's production source.
  cp "$ROOT/ThunderPipe.Core.IntegrationTests/ci/minio.Dockerfile" \
    "$TS_DIR/minio/Dockerfile"
  sed -i -e 's/mc anonymous set download/mc policy set download/g' \
    -e 's#^source /usr/bin/docker-entrypoint.sh#exec /usr/bin/docker-entrypoint.sh#' \
    -e 's/\r$//' "$TS_DIR/minio/thunderstore-entrypoint.sh"
  chmod +x "$TS_DIR/minio/thunderstore-entrypoint.sh"

  # The CI does not start Remix; package downloads must be forwarded to
  # Django, exactly as in the locally validated Thunderstore setup.
  python3 - "$TS_DIR/nginx/conf/default.conf" <<'NGINX_CI_PATCH'
from pathlib import Path
import sys

path = Path(sys.argv[1])
config = path.read_text(encoding='utf-8')
config = config.replace(
    '    location /api/             { proxy_pass http://django:8000; }',
    '    location /api/             { proxy_pass http://django:8000; }\n'
    '    location /package/download/ { proxy_pass http://django:8000; }',
)
path.write_text(config, encoding='utf-8')
NGINX_CI_PATCH

  # CI-only workaround for Thunderstore issue #1343.
  sed -i \
    's/validate_markdown(serializer.validated_data\["readme_data"\])/validate_markdown(filename="README.md", readme_data=serializer.validated_data["readme_data"])/' \
    "$TS_DIR/django/thunderstore/repository/api/experimental/views/validators.py"

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

  # Run the readiness probe in the worker's own environment, and preserve
  # diagnostics rather than hiding Celery's stderr. Do not mistake a running
  # container for a functioning task queue.
  local celery_diag="$ART_DIR/celery-readiness.log"
  local ping_output
  : > "$celery_diag"
  ready=0
  for i in $(seq 1 30); do
    if ! compose ps --status running django-worker | grep -q 'django-worker'; then
      echo "Attempt $i: django-worker container is not running" >> "$celery_diag"
    else
      ping_output="$(compose exec -T django-worker celery -A thunderstore.core inspect ping --timeout=10 2>&1)" || true
      if grep -q 'pong' <<< "$ping_output"; then
        ready=1
        echo "Attempt $i: worker responded to inspect ping" >> "$celery_diag"
        break
      fi
      printf 'Attempt %s: worker inspect ping returned:\n%s\n' "$i" "$ping_output" >> "$celery_diag"
    fi
    sleep 5
  done
  if [[ "$ready" -ne 1 ]]; then
    echo 'Celery worker did not respond to inspect ping before the deadline' >&2
    echo '--- Worker container status ---' | tee -a "$celery_diag" >&2
    compose ps django-worker rabbitmq 2>&1 | tee -a "$celery_diag" >&2 || true
    echo '--- Django-side Celery inspect (comparison) ---' | tee -a "$celery_diag" >&2
    compose exec -T django celery -A thunderstore.core inspect ping --timeout=10 \
      2>&1 | tee -a "$celery_diag" >&2 || true
    echo '--- Recent worker and broker logs ---' | tee -a "$celery_diag" >&2
    compose logs --no-color --tail=100 django-worker rabbitmq \
      2>&1 | tee -a "$celery_diag" >&2 || true
    echo '--- Worker probe attempts (last 80 lines) ---' >&2
    tail -n 80 "$celery_diag" >&2
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
