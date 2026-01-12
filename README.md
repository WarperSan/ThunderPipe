<br>
<div align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
    <h1>ThunderPipe</h1>
    <div>
        <a href="https://www.nuget.org/packages/ThunderPipe"><img alt="NuGet Badge" src="https://img.shields.io/nuget/v/ThunderPipe"></a>
        <a href="https://www.nuget.org/packages/ThunderPipe"><img alt="NuGet Version" src="https://img.shields.io/nuget/dt/ThunderPipe"></a>
        <a href="https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE"><img alt="License" src="https://img.shields.io/github/license/WarperSan/ThunderPipe"></a>
    </div>
</div>

## Overview

ThunderPipe is a command-line tool for validating and publishing mod packages to [Thunderstore](https://thunderstore.io/).

## Installation

You can install ThunderPipe from NuGet by running the following command:

```bash
dotnet tool install ThunderPipe
```

> [!NOTE]
> As this package is in beta, you will need to add the flag `--prerelease`.

Alternatively, you can install it by downloading the package from [NuGet](https://www.nuget.org/packages/ThunderPipe/latest) or [GitHub Releases](https://github.com/WarperSan/ThunderPipe/releases/latest) directly, and installing the local file:

```bash
dotnet tool install ThunderPipe --add-source <path-to-download>
```

> [!IMPORTANT]
> The commands shown in further sections will omit the `dotnet tool run` part, due to being installed globally. This can be achieved by adding the `--global` flag in the installation command.

## Usage

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

### Validating a Community

Checks if your community slug matches with an existing community.

```bash
ThunderPipe validate community <community>
```

| Argument      | Description           |
|---------------|-----------------------|
| `<community>` | Slug of the community |

Modding communities often provide a template that you can use that includes this value. However, if you need to find it yourself, you can look at this [API endpoint](https://thunderstore.io/api/experimental/community/) for the `identifier` you are looking for.

### Validating Categories

Checks if your categories' slugs match existing categories in your community.

```bash
ThunderPipe validate categories <community> \
	--categories <category1> \
	--categories <category2>
```

| Argument       | Description             |
|----------------|-------------------------|
| `<community>`  | Slug of the community   |
| `<categories>` | Slugs for each category |

Modding communities often provide a lookup table to see the slug of every category. However, if you need to find it yourself, you can look at this [API endpoint](https://thunderstore.io/api/experimental/community/<COMMUNITY>/category/) for the `slug` you are looking for.

> [!NOTE]
> If you use the endpoint, make sure to replace `<COMMUNITY>` with the slug of the community you are within.

### Validating Dependencies

Checks if your dependencies' strings match existing packages.

```bash
ThunderPipe validate dependencies <dependencies>
```

| Argument         | Description                            |
|------------------|----------------------------------------|
| `<dependencies>` | Dependency strings for each dependency |

### Validating a Package

Checks if your package meets [Thunderstore's requirements](https://thunderstore.io/package/create/docs/) before you attempt to publish it.

```bash
ThunderPipe validate package <package-folder>
```

| Argument           | Description                                       |
|--------------------|---------------------------------------------------|
| `<package-folder>` | Path to the folder containing the package's files |

## Why ThunderPipe instead of TCLI?

While the official [Thunderstore CLI (TCLI)](https://github.com/thunderstore-io/thunderstore-cli) is excellent for general users, ThunderPipe is built **by modders for modders**. who prefer a "do one thing and do it well" philosophy.

<details>
    <summary><b>Leaner Tooling</b></summary>
	<p>
		TCLI is a mod installer, mod manager and mod publisher. If you already use applications like <a href="https://galemodmanager.com/"><i>Gale</i></a> or <a href="https://r2modman.com"><i>r2modman</i></a>, ThunderPipe provides a lighter footprint for your development environment.
	</p>
</details>

<details>
    <summary><b>Structured Workflow</b></summary>
	<p>
    	ThunderPipe is built to solve the lack of a standardized automation workflow. It provides clear exit codes and validation steps ideal for automated CI/CD pipelines.
	</p>
</details>

## Contributing

Contributions are welcome! If you encounter a bug or have a feature request, please [open an issue](https://github.com/WarperSan/ThunderPipe/issues/new).

<div align="center">
    <sub>
        <hr>
		Made with ❤️ for the modding community
    </sub>
</div>
