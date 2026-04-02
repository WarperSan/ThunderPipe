<p align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
</p>

# ThunderPipe
[![NuGet Badge](https://img.shields.io/nuget/v/ThunderPipe)](https://www.nuget.org/packages/ThunderPipe)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ThunderPipe?color=purple)](https://www.nuget.org/packages/ThunderPipe)
[![License](https://img.shields.io/github/license/WarperSan/ThunderPipe)](https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE)

## Overview

ThunderPipe is a command-line tool for validating and publishing mod packages to [Thunderstore](https://thunderstore.io/).

## Installation

You can install ThunderPipe from NuGet by running the following command:

```bash
dotnet tool install ThunderPipe
```

Alternatively, you can install it by downloading the package from [NuGet](https://www.nuget.org/packages/ThunderPipe/latest) or [GitHub Releases](https://github.com/WarperSan/ThunderPipe/releases/latest) directly, and installing the local file:

```bash
dotnet tool install ThunderPipe --add-source <path-to-download>
```

> [!IMPORTANT]
> The commands shown in further sections will omit the `dotnet tool run` part, due to [being installed globally](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-use#use-the-tool-as-a-global-tool-traditional-installation). This can be achieved by adding the `--global` flag in the installation command.

## Commands

### Publishing a Package

Uploads your `.zip` file directly to Thunderstore.

```bash
ThunderPipe publish <file> <team> <community> --token <your-token>
```

| Argument       | Description                                    |
|----------------|------------------------------------------------|
| `<file>`       | Path to your `.zip` file                       |
| `<team>`       | Name of the team that owns the service account |
| `<community>`  | Slug of the community                          |
| `<your-token>` | API token of the service account to use        |

### Validating a Package

Checks if your package meets [Thunderstore's requirements](https://thunderstore.io/package/create/docs/) before you attempt to publish it.

```bash
ThunderPipe validate package <package-folder>
```

| Argument           | Description                                       |
|--------------------|---------------------------------------------------|
| `<package-folder>` | Path to the folder containing the package's files |

### Create a Manifest

Creates a `manifest.json` file that meets [Thunderstore's requirements](https://thunderstore.io/package/create/docs/).

```bash
ThunderPipe create manifest <name> <version>
```

| Argument    | Description            |
|-------------|------------------------|
| `<name>`    | Name of the package    |
| `<version>` | Version of the package |

## GitHub Action

[upload-thunderstore-package](https://github.com/WarperSan/upload-thunderstore-package) is a GitHub Action that wraps ThunderPipe to automate the full publish pipeline. Under the hood, the action automates the full validation, the build, the packaging and the publishing.

## Wiki

If you want more in-depth information about this tool, you can visit the [wiki](https://github.com/WarperSan/ThunderPipe/wiki).

## Contributing

Contributions are welcome! If you encounter a bug or have a feature request, please [open an issue](https://github.com/WarperSan/ThunderPipe/issues/new).

<div align="center">
    <sub>
        <hr>
		Made with ❤️ for the modding community
    </sub>
</div>
