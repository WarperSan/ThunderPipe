<p align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
</p>

# ThunderPipe.Sdk
[![NuGet Badge](https://img.shields.io/nuget/v/ThunderPipe.Sdk)](https://www.nuget.org/packages/ThunderPipe.Sdk)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ThunderPipe.Sdk?color=purple)](https://www.nuget.org/packages/ThunderPipe.Sdk)
[![License](https://img.shields.io/github/license/WarperSan/ThunderPipe)](https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE)

## Overview

ThunderPipe.Sdk is the MSBuild integration
of [ThunderPipe.Core](https://github.com/WarperSan/ThunderPipe/tree/master/ThunderPipe.Core).

## Installation

Go to [NuGet](https://www.nuget.org/packages/ThunderPipe.Sdk), and copy the Sdk XML node for the latest version into your project:

```xml
<Sdk Name="ThunderPipe.Sdk" Version="version.number.here" />
```

## Tasks

### ThunderPipePack

Packs your files into a valid Thunderstore `.zip` package.

Use pre-existing ThunderPipePack Target that executes after Build:

```xml
<PropertyGroup>
  <RunThunderPipePackAfterBuild>true</RunThunderPipePackAfterBuild>
</PropertyGroup>
```

Or use the ThunderPipePack Task in your own Target:

```xml
<ThunderPipePack
  Token="$(ThunderstoreToken)"
  Team="$(ThunderstoreTeam)"
  Name="$(ThunderstoreName)"
  Version="$(ThunderstoreVersion)"
  Description="$(ThunderstoreDescription)"
  Website="$(ThunderstoreWebsite)"
  Dependencies="@(ThunderstoreDependency)"
  PackageFiles="@(ThunderstorePackageFile)"
  Files="@(ThunderstoreFile)"
  Host="$(ThunderstoreHost)"
  OutputDir="$(ThunderPipeOutputDir)"
  OutputFile="$(ThunderPipeOutputFile)"
>
  <Output TaskParameter="OutputDir" PropertyName="ThunderPipeOutputDir" />
  <Output TaskParameter="OutputFile" PropertyName="ThunderPipeOutputFile" />
</ThunderPipePack>
```

| Argument  | Description                                    |
|-----------|------------------------------------------------|
| `Token`   | API token of the service account to use        |
| `Team`    | Name of the team that owns the service account |
| `Name`    | Name of the package                            |
| `Version` | Version of the package                         |
| `Description` | Description of the package |
| `Website` | The website for the package project |
| `Dependencies` | A list of Thunderstore dependencies |
| `PackageFiles` | Metadata files for the package |
| `Files` | Files to include in the package |
| `Host` | Thunderstore repository URL (optional) for package validation if `Token` is set  |
| `OutputDir`| Where the package will be built |
| `OutputFile` | The full path for the package (overrides `OutputDir` if set) |

### ThunderPipePublish

Uploads your `.zip` file directly to Thunderstore.

Use pre-existing ThunderPipePublish Target that executes after Build:

```xml
<PropertyGroup>
  <RunThunderPipePublishAfterBuild>true</RunThunderPipePublishAfterBuild>
</PropertyGroup>
```

Or use the ThunderPipePublish Task in your own Target:

```xml
<ThunderPipePublish
  Token="$(ThunderstoreToken)"
  File="$(ThunderPipeOutputFile)"
  Team="$(ThunderstoreTeam)"
  Communities="$(ThunderstoreCommunities)"
  Categories="$(ThunderstoreCategories)"
  CommunityCategories="$(ThunderstoreCommunityCategories)"
  Host="$(ThunderstoreHost)"
  HasNSFW="$(ThunderstoreNSFW)"
>
  <Output TaskParameter="Output" PropertyName="ThunderPipePublishDownloadUrl" />
</ThunderPipePublish>
```

| Argument | Description                                    |
|----------|------------------------------------------------|
| `Token`  | API token of the service account to use        |
| `File`   | Path to your `.zip` package                    |
| `Team`   | Name of the team that owns the service account |
| `Communities` | A list of community slugs to upload to |
| `Categories` | A list of categories that apply to all communities to upload to |
| `CommunityCategories` | A list of communities and their individual categories |
| `Host` | Thunderstore repository URL to upload the package to (optional) |
| `HasNSFW` | Whether or not the package should be marked as NSFW |

## Default Values

The SDK generates several Thunderstore properties that can be use in tasks.

Here is a list of each property, with their defaults in order:

- `$(ThunderstoreName)`
  - `$(Product)`
  - `$(AssemblyName)`
  - `$(RootNamespace)`
- `$(ThunderstoreVersion)`
  - `$(Version)`
- `$(ThunderstoreDescription)`
  - `$(Description)`
- `$(ThunderstoreTeam)`
  - `$(Authors)`
- `$(ThunderstoreWebsite)`
  - `$(RepositoryUrl)`
  - `$(PackageProjectUrl)`
- `$(ThunderstoreNSFW)`
  - `false`
- `$(ThunderstoreHost)`
  - `https://thunderstore.io/`
- `$(ThunderPipeOutputDir)`
  - if `$(ArtifactsPath)` exists: `$(ArtifactsPath)/thunderstore/$(ArtifactsPivots)`
  - `$(TargetDir)thunderstore/`

You can see more [here](https://github.com/WarperSan/ThunderPipe/blob/master/ThunderPipe.Sdk/Sdk/Sdk.targets).

## Examples

```xml
<!-- Thunderstore package properties -->
<PropertyGroup>
  <ThunderstoreTeam>TS_Team</ThunderstoreTeam>
  <ThunderstoreName>Some_Plugin</ThunderstoreName>
  <Version>0.1.0</Version>
  <Description>Short description.</Description>
  <RepositoryUrl>https://example.com/</RepositoryUrl>

  <!-- MSBuild arrays use ';' as a separator -->
  <ThunderstoreCommunities>lethal-company;repo;peak</ThunderstoreCommunities>

  <!--
    These categories are applied to all defined communities,
    even to communities only used as keys in ThunderstoreCommunityCategories,
    which also get added to communities to upload to.
  -->
  <ThunderstoreCategories>mods;libraries</ThunderstoreCategories>

  <!-- This is the format for adding per-community categories -->
  <ThunderstoreCommunityCategories>
    repo=client-side/server-side;
    lethal-company=bepinex;
    yapyap=all-clients
  </ThunderstoreCommunityCategories>

  <!-- Build Thunderstore package automatically after build -->
  <RunThunderPipePackAfterBuild>true</RunThunderPipePackAfterBuild>

  <!--
    Optionally also publish automatically, for example with a condition:
    dotnet build -c Release -p:PublishTS=true
  -->
  <RunThunderPipePublishAfterBuild
    Condition="'$(PublishTS)' == 'true'"
  >true</RunThunderPipePublishAfterBuild
  >

  <!--
	  Optionally configure exact location & file name for zip
    <ThunderPipeOutputFile>./bin/file.zip</ThunderPipeOutputFile>
  -->
</PropertyGroup>

<!-- Files to add to the package -->
<ItemGroup>
  <!-- $(TargetPath) is the dll built by the csproj -->
  <ThunderstoreFile Include="$(TargetPath)" Destination="plugins/" />
  <ThunderstorePackageFile Include="./README.md" />
  <ThunderstorePackageFile Include="./CHANGELOG.md" />
  <ThunderstorePackageFile Include="./icon.png" />
  <ThunderstorePackageFile Include="./LICENSE" />
</ItemGroup>

<!-- Thunderstore dependencies -->
<ItemGroup>
  <ThunderstoreDependency Include="BepInEx-BepInExPack" Version="5.4.2305" />
</ItemGroup>
```
