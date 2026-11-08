<p align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
</p>

# ThunderPipe
[![NuGet Badge](https://img.shields.io/nuget/v/ThunderPipe)](https://www.nuget.org/packages/ThunderPipe)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ThunderPipe?color=purple)](https://www.nuget.org/packages/ThunderPipe)
[![License](https://img.shields.io/github/license/WarperSan/ThunderPipe)](https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE)

ThunderPipe is a command-line tool for validating and publishing mod packages to [Thunderstore](https://thunderstore.io/).

## Installation

Install ThunderPipe from NuGet:

```bash
dotnet tool install ThunderPipe
```

Alternatively, download the package from [NuGet](https://www.nuget.org/packages/ThunderPipe/latest) or [GitHub Releases](https://github.com/WarperSan/ThunderPipe/releases/latest) and install it from a local file:

```bash
dotnet tool install ThunderPipe --add-source <path-to-download>
```

> [!IMPORTANT]
> The commands shown below omit `dotnet tool run` assuming a [global installation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-use#use-the-tool-as-a-global-tool-traditional-installation). Add the `--global` flag to your install command to enable this.

## Commands

### Publish

Uploads a `.zip` package directly to Thunderstore.

```bash
ThunderPipe publish <file> <team> <community> --token <your-token>
```

| Argument | Description |
|---|---|
| `<file>` | Path to your `.zip` file |
| `<team>` | Name of the team that owns the service account |
| `<community>` | Slug of the community to publish to |
| `<your-token>` | API token of the service account to use |

### Validate

Checks if a package meets [Thunderstore's requirements](https://thunderstore.io/package/create/docs/) before publishing.

```bash
ThunderPipe validate package <package-folder>
```

| Argument | Description |
|---|---|
| `<package-folder>` | Path to the folder containing the package files |

### Create Manifest

Generates a `manifest.json` that meets [Thunderstore's requirements](https://thunderstore.io/package/create/docs/).

```bash
ThunderPipe create manifest <name> <version>
```

| Argument | Description |
|---|---|
| `<name>` | Name of the package |
| `<version>` | Version of the package |
