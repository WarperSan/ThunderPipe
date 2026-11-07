import json
import sys
from pathlib import Path

raw = Path(sys.argv[1]).read_text(encoding="utf-8-sig")
output = Path(sys.argv[2])
# The seed script prints a compact JSON object on its final stdout line.
# Django shell may print an introductory line first.
for line in reversed(raw.splitlines()):
    line = line.strip()
    if not line.startswith("{"):
        continue
    try:
        value = json.loads(line)
    except json.JSONDecodeError:
        continue
    if isinstance(value, dict) and all(k in value for k in
        ("token", "team", "community", "category", "seedPackage", "seedVersion", "oldVersion")):
        output.write_text(json.dumps(value, ensure_ascii=False), encoding="utf-8")
        print("Fixture JSON extracted successfully.")
        break
else:
    output.unlink(missing_ok=True)
    raise SystemExit("No valid fixture JSON in Django output. Inspect artifacts/django-seed-raw.log")
