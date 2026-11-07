"""Execute INSIDE Django after tests to record read-only server-side state.
No test outcome is invented here: missing/failed uploads remain visible.
"""
import hashlib
import json
from thunderstore.repository.models import PackageVersion
from thunderstore.usermedia.models import UserMedia

versions = []
for pv in PackageVersion.objects.filter(
        package__owner__name="ThunderPipeIntegration",
        name__startswith="TPIntegration").order_by("-id")[:10]:
    record = {
        "name": pv.name,
        "version": pv.version_number,
        "is_active": pv.is_active,
        "bytes": pv.file_size,
        "stored_zip_sha256": None,
        "storage_error": None,
    }
    try:
        sha = hashlib.sha256()
        with pv.file.open("rb") as f:
            for block in iter(lambda: f.read(1024 * 1024), b""):
                sha.update(block)
        record["stored_zip_sha256"] = sha.hexdigest().upper()
    except Exception as exc:
        record["storage_error"] = str(exc)
    versions.append(record)

uploads = []
for media in UserMedia.objects.filter(filename="package.zip").order_by("-datetime_created")[:12]:
    uploads.append({
        "uuid": str(media.uuid),
        "status": str(media.status),
        "size": media.size,
        "filename": media.filename,
    })
print(json.dumps({"published_packages": versions, "uploads": uploads}, indent=2))
