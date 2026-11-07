"""Run INSIDE the local Thunderstore Django service using manage.py shell.

Creates isolated, repeatable API fixtures and a fresh local service-account token.
STDOUT contains only the JSON configuration consumed by the C# test runner.
"""
import json

from django.contrib.auth import get_user_model
from thunderstore.account.models import ServiceAccount
from thunderstore.community.models import Community, PackageCategory
from thunderstore.repository.models import Package, PackageVersion, Team

TEAM = "ThunderPipeIntegration"
COMMUNITY = "thunderpipe-integration"
CATEGORY = "tp-integration-primary"
PACKAGE = "TPSeedPackage"
ACTIVE_VERSION = "1.2.3"
INACTIVE_VERSION = "1.2.2"

team = Team.objects.filter(name=TEAM).first()
if team is None:
    team = Team.create(TEAM)

community, _ = Community.objects.get_or_create(
    identifier=COMMUNITY,
    defaults={"name": "ThunderPipe Integration Test", "is_listed": True,
              "require_package_listing_approval": False, "block_auto_updates": False},
)
Community.objects.filter(pk=community.pk).update(
    is_listed=True, require_package_listing_approval=False, block_auto_updates=False
)
category, _ = PackageCategory.objects.get_or_create(
    community=community, slug=CATEGORY, defaults={"name": "Integration Primary"}
)

# Generate 105 deterministic, *local* records to test real cursor pagination
# (the experimental community/category API returns 100 records per page).
for i in range(105):
    Community.objects.get_or_create(
        identifier=f"tp-integration-page-{i:03d}",
        defaults={"name": f"ThunderPipe Page Fixture {i:03d}", "is_listed": True},
    )
    PackageCategory.objects.get_or_create(
        community=community, slug=f"tp-integration-page-{i:03d}",
        defaults={"name": f"Integration Page Fixture {i:03d}"},
    )

package, _ = Package.objects.get_or_create(
    owner=team, namespace=team.get_namespace(), name=PACKAGE
)
for version, active in ((INACTIVE_VERSION, False), (ACTIVE_VERSION, True)):
    PackageVersion.objects.update_or_create(
        package=package, version_number=version,
        defaults={
            "name": PACKAGE,
            "description": "Local ThunderPipe client fixture",
            "website_url": "https://example.org",
            "file_size": 1,
            "file": f"repository/packages/tp-integration-{version}.zip",
            "icon": "repository/icons/tp-integration.png",
            "readme": "# Integration Test\n",
            "is_active": active,
        },
    )
package.latest = PackageVersion.objects.get(package=package, version_number=ACTIVE_VERSION)
package.save(update_fields=["latest"])

admin = get_user_model().objects.get(username="admin")
for previous in ServiceAccount.objects.filter(owner=team, user__first_name="TPIntegrationTest"):
    previous.delete()
_, token = ServiceAccount.create(team, "TPIntegrationTest", admin)

print(json.dumps({
    "token": token,
    "team": TEAM,
    "community": COMMUNITY,
    "category": CATEGORY,
    "seedPackage": PACKAGE,
    "seedVersion": ACTIVE_VERSION,
    "oldVersion": INACTIVE_VERSION,
}))
