@echo off
setlocal
cd /d "%~dp0.."
if not defined THUNDERSTORE_DIR set "THUNDERSTORE_DIR=D:\Thunderstore"
if not defined THUNDERPIPE_BASE_URL set "THUNDERPIPE_BASE_URL=http://thunderstore.localhost/"
set "THUNDERPIPE_FIXTURE_PATH=%CD%\artifacts\thunderpipe-fixture.json"
set "THUNDERPIPE_HTTP_TRACE=%CD%\artifacts\client-http.log"
if not exist "artifacts" mkdir "artifacts"

echo [1/3] Seed local Thunderstore Django fixtures...
docker compose --project-directory "%THUNDERSTORE_DIR%" -f "%THUNDERSTORE_DIR%\docker-compose.yml" exec -T django python manage.py shell -c "exec(__import__('sys').stdin.read())" < "scripts\seed_thunderstore.py" > "artifacts\django-seed-raw.log"
if errorlevel 1 exit /b 1
python -c "import json,sys; lines=open(sys.argv[1],encoding='utf-8-sig').read().splitlines(); data=next(json.loads(line) for line in reversed(lines) if line.lstrip().startswith('{')); open(sys.argv[2],'w',encoding='utf-8').write(json.dumps(data))" "artifacts\django-seed-raw.log" "artifacts\thunderpipe-fixture.json"
if errorlevel 1 exit /b 1

echo [2/3] Run C# HTTP integration tests with dotnet test...
dotnet test ThunderPipe.Core.IntegrationTests\ThunderPipe.Core.IntegrationTests.csproj --logger "trx;LogFileName=integration.trx" --results-directory artifacts --verbosity normal
set "TEST_EXIT=%errorlevel%"

echo [3/3] Collect Thunderstore logs and read-only state...
docker compose --project-directory "%THUNDERSTORE_DIR%" -f "%THUNDERSTORE_DIR%\docker-compose.yml" logs --since 20m --no-color nginx django minio > "artifacts\docker-server.log"
docker compose --project-directory "%THUNDERSTORE_DIR%" -f "%THUNDERSTORE_DIR%\docker-compose.yml" exec -T django python manage.py shell -c "exec(__import__('sys').stdin.read())" < "scripts\verify_server_state.py" > "artifacts\server-state.json"
echo Results: artifacts\integration.trx, client-http.log, docker-server.log, server-state.json
exit /b %TEST_EXIT%
