# The repo selects .NET 10's MTP test driver while the test target is net8.0.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS net8-runtime
FROM mcr.microsoft.com/dotnet/sdk:10.0
COPY --from=net8-runtime /usr/share/dotnet/shared/ /usr/share/dotnet/shared/
ENV HUSKY=0
WORKDIR /src
COPY . .
RUN dotnet restore ThunderPipe.Core.IntegrationTests/ThunderPipe.Core.IntegrationTests.csproj
RUN dotnet build ThunderPipe.Core.IntegrationTests/ThunderPipe.Core.IntegrationTests.csproj \
    --configuration Release --no-restore
