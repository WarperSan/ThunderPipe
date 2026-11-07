#!/usr/bin/env python3
"""Check stable Thunderstore releases and persist discovery state in a dedicated Git branch.

Only release discovery is implemented: no compatibility test is run or marked successful.
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

UPSTREAM = "thunderstore-io/Thunderstore"
STATE_FILE = "thunderstore-release-state.json"
API = "https://api.github.com"


def request(method, path, data=None):
    url = API + path
    headers = {
        "Accept": "application/vnd.github+json",
        "User-Agent": "ThunderPipe-release-check",
        "X-GitHub-Api-Version": "2022-11-28",
        "Authorization": "Bearer " + os.environ["GH_TOKEN"],
    }
    body = json.dumps(data).encode() if data is not None else None
    if body is not None:
        headers["Content-Type"] = "application/json"
    req = urllib.request.Request(url, data=body, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            return json.load(response)
    except urllib.error.HTTPError as error:
        detail = error.read().decode(errors="replace")[:800]
        if error.code == 404:
            return None
        raise RuntimeError(f"GitHub API {method} {path}: HTTP {error.code}: {detail}") from error


def quoted(value):
    return urllib.parse.quote(value, safe="")


def read_state(repo, branch):
    path = f"/repos/{repo}/contents/{STATE_FILE}?ref={quoted(branch)}"
    file = request("GET", path)
    if file is None:
        return None, None
    decoded = base64.b64decode(file["content"]).decode("utf-8")
    return json.loads(decoded), file["sha"]


def ensure_branch(repo, branch):
    ref_path = f"/repos/{repo}/git/ref/heads/{quoted(branch)}"
    if request("GET", ref_path) is not None:
        return
    info = request("GET", f"/repos/{repo}")
    default = info["default_branch"]
    base = request("GET", f"/repos/{repo}/git/ref/heads/{quoted(default)}")
    try:
        request("POST", f"/repos/{repo}/git/refs", {
            "ref": f"refs/heads/{branch}", "sha": base["object"]["sha"]
        })
    except RuntimeError:
        # Another run might have created it concurrently.
        if request("GET", ref_path) is None:
            raise


def save_state(repo, branch, state, sha):
    content = json.dumps(state, ensure_ascii=False, indent=2, sort_keys=True) + "\n"
    payload = {
        "message": "chore: record Thunderstore release discovery",
        "content": base64.b64encode(content.encode()).decode(),
        "branch": branch,
    }
    if sha:
        payload["sha"] = sha
    request("PUT", f"/repos/{repo}/contents/{STATE_FILE}", payload)


def compare_api_changes(previous_tag, latest_tag):
    # GitHub Compare API paginates files for large diffs; never silently skip tests
    # if the comparison cannot be evaluated safely.
    path = f"/repos/{UPSTREAM}/compare/{quoted(previous_tag)}...{quoted(latest_tag)}?per_page=100"
    result = request("GET", path)
    if result is None:
        raise RuntimeError("Release comparison unavailable; refusing to classify it as unchanged")
    files = result.get("files", [])
    if result.get("total_commits", 0) > 250 or len(files) >= 300:
        return True, "comparison too large; conservatively require tests"
    if result.get("status") not in ("ahead", "identical"):
        return True, "non-linear release history; conservatively require tests"
    for file in files:
        names = [file.get("filename", ""), file.get("previous_filename", "")]
        if any("api" in name.lower().split("/") or "model" in name.lower().split("/") or "models" in name.lower().split("/") for name in names):
            return True, "API/model path changed"
    return False, "no API/model directory changes detected"


def output(key, value):
    print(f"{key}={value}")
    if os.getenv("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as file:
            file.write(f"{key}={value}\n")


def main():
    repo = os.environ["REPOSITORY"]
    branch = os.getenv("STATE_BRANCH", "thunderstore-integration-state")
    release = request("GET", f"/repos/{UPSTREAM}/releases/latest")
    if not release:
        raise RuntimeError("Thunderstore has no latest stable GitHub release")
    if release.get("draft") or release.get("prerelease"):
        raise RuntimeError("Unexpected draft/prerelease returned by releases/latest")
    tag = release["tag_name"]
    ensure_branch(repo, branch)
    state, sha = read_state(repo, branch)
    state = state or {
        "last_checked_release": None,
        "last_successful_test_release": None,
        "pending_test_release": None,
        "last_check_result": None,
    }
    previous = state.get("last_checked_release")
    should_test = False
    if previous == tag:
        reason = "release already checked"
    elif previous is None:
        # First execution: establish a baseline, do not pretend a test was run.
        reason = "first execution: baseline recorded; no tests run"
    else:
        should_test, reason = compare_api_changes(previous, tag)

    state["last_checked_release"] = tag
    state["last_check_result"] = {
        "checked_at": datetime.now(timezone.utc).isoformat(),
        "release": tag,
        "previous_release": previous,
        "requires_integration_tests": should_test,
        "reason": reason,
        "integration_tests_executed": False,
    }
    if should_test:
        state["pending_test_release"] = tag
    # Do not change last_successful_test_release: no real tests are executed.
    save_state(repo, branch, state, sha)
    output("release_tag", tag)
    output("should_test", str(should_test).lower())
    print(json.dumps(state["last_check_result"], ensure_ascii=False, indent=2))
    if should_test:
        print("WARNING: API/model changes detected, but integration tests are not yet implemented.")


if __name__ == "__main__":
    try:
        main()
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        sys.exit(1)
