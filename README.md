<br>
<div align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
    <h1>ThunderPipe</h1>
    <div>
        <a href="https://www.nuget.org/packages/WarperSan.ThunderPipe"><img alt="NuGet Badge" src="https://img.shields.io/nuget/v/WarperSan.ThunderPipe.svg"></a>
        <a href="https://www.nuget.org/packages/WarperSan.ThunderPipe"><img alt="NuGet Version" src="https://img.shields.io/nuget/dt/WarperSan.ThunderPipe.svg"></a>
        <a href="https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE"><img alt="License" src="https://img.shields.io/github/license/WarperSan/ThunderPipe"></a>
    </div>
</div>

ThunderPipe is a command-line tool for building, validating and publishing mod packages to [Thunderstore](https://thunderstore.io/).

## Why this instead of TCLI?

While the official [Thunderstore CLI (TCLI)](https://github.com/thunderstore-io/thunderstore-cli) is excellent for general users, **ThunderPipe** is built for developers who prefer a "do one thing and do it well" philosophy.

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

## Installation

Install ThunderPipe globally via NuGet:

```bash
dotnet tool install --global ThunderPipe
```

Once installed, you can run the tool using:

```bash
ThunderPipe --help
```

## Usage

### Validating a Package

Check if your package meets [Thunderstore's requirements](https://thunderstore.io/package/create/docs/) before you attempt to publish.

```bash
ThunderPipe validate <package-folder>
```

| Argument           | Description                                       |
|--------------------|---------------------------------------------------|
| `<package-folder>` | Path to the folder containing the package's files |

### Publishing a Package

Upload your `.zip` package directly to Thunderstore for a specific team in a specific community.

```bash
ThunderPipe publish <file> <team> <community> --token <your-token>
```

| Argument       | Description                             |
|----------------|-----------------------------------------|
| `<file>`       | Path to the file to publish             |
| `<team>`       | The team name on Thunderstore           |
| `<community>`  | The communiy slug                       |
| `<your-token>` | API token of the service account to use |

## Contributing

Contributions are welcome! If you encounter a bug or have a feature request, please [open an issue](https://github.com/WarperSan/ThunderPipe/issues/new).

<div align="center">
    <sub>
        <hr>
		Made with ❤️ for the modding community
    </sub>
</div>
