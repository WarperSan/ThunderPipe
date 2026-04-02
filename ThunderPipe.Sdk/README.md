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

You can install and use ThunderPipe.Sdk by adding the following line to your `.csproj`:

```msbuild

<Sdk Name="ThunderPipe.Sdk"/>
```

## Tasks

### ThunderPipePack

Packs your files into a valid `.zip` package.

```msbuild

<ThunderPipePack
	Token="your-token"
	Team="team"
	Name="name"
	Version="version"
/>
```

| Argument  | Description                                    |
|-----------|------------------------------------------------|
| `Token`   | API token of the service account to use        |
| `Team`    | Name of the team that owns the service account |
| `Name`    | Name of the package                            |
| `Version` | Version of the package                         |

### ThunderPipePublish

Uploads your `.zip` file directly to Thunderstore.

```msbuild

<ThunderPipePublish
	Token="your-token"
	Team="team"
	File="your-file.zip"
/>
```

| Argument | Description                                    |
|----------|------------------------------------------------|
| `Token`  | API token of the service account to use        |
| `Team`   | Name of the team that owns the service account |
| `File`   | Path to your `.zip` package                    |

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

You can see more [here](https://github.com/WarperSan/ThunderPipe/blob/master/ThunderPipe.Sdk/Sdk/Sdk.targets).
