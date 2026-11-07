@echo off
setlocal
cd /d "%~dp0.."
if not defined THUNDERSTORE_DIR set "THUNDERSTORE_DIR=D:\Thunderstore"
if not defined THUNDERPIPE_BASE_URL set "THUNDERPIPE_BASE_URL=http://thunderstore.localhost/"
set "THUNDERPIPE_FIXTURE_PATH=%CD%\artifacts\thunderpipe-fixture.json"
set "THUNDERPIPE_HTTP_TRACE=%CD%\artifacts\client-http.log"
if not exist "artifacts" mkdir "artifacts"
echo [1/4] Seed isolated Django integration fixtures...
docker compose --project-directory "%THUNDERSTORE_DIR%" -f "%THUNDERSTORE_DIR%\docker-compose.yml" exec -T django python manage.py shell -c "exec(__import__('sys').stdin.read())" < "scripts\seed_thunderstore.py" > "artifacts\django-seed-raw.log"
if errorlevel 1 exit /b 1
python "scripts\extract_fixture_json.py" "artifacts\django-seed-raw.log" "artifacts\thunderpipe-fixture.json"
if errorlevel 1 exit /b 1
echo [2/4] Run actual C# HTTP integration tests...
dotnet test ThunderPipe.Core.IntegrationTests\ThunderPipe.Core.IntegrationTests.csproj --logger "trx;LogFileName=integration.trx" --results-directory artifacts --verbosity normal
set "TEST_EXIT=%errorlevel%"
echo [3/4] Capture real Docker service logs and published object state...
docker compose --project-directory "%THUNDERSTORE_DIR%" -f "%THUNDERSTORE_DIR%\docker-compose.yml" logs --since 20m --no-color nginx django minio > "artifacts\docker-server.log"
docker compose --project-directory "%THUNDERSTORE_DIR%" -f "%THUNDERSTORE_DIR%\docker-compose.yml" exec -T django python manage.py shell -c "exec(__import__('sys').stdin.read())" < "scripts\verify_server_state.py" > "artifacts\server-state.json"
echo [4/4] See artifacts\integration.trx, client-http.log, docker-server.log, server-state.json
echo Test exit code: %TEST_EXIT%
exit /b %TEST_EXIT%
