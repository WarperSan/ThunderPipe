#!/usr/bin/env python3
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
