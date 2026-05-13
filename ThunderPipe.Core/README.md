<p align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
</p>

# ThunderPipe.Core
[![NuGet Badge](https://img.shields.io/nuget/v/ThunderPipe.Core)](https://www.nuget.org/packages/ThunderPipe.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ThunderPipe.Core?color=purple)](https://www.nuget.org/packages/ThunderPipe.Core)
[![License](https://img.shields.io/github/license/WarperSan/ThunderPipe)](https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE)
[![Codecov Badge](https://codecov.io/github/WarperSan/ThunderPipe/graph/badge.svg)](https://codecov.io/github/WarperSan/ThunderPipe)

ThunderPipe.Core is the shared logic that powers every [ThunderPipe](https://github.com/WarperSan/ThunderPipe) project.

## Building on ThunderPipe.Core

If the existing tools don't meet your needs, you can build your own by referencing this package. Doing so gives you full access to everything `ThunderPipe.Core` exposes.

> [!TIP]
> Before building a custom integration, consider [opening an issue](https://github.com/WarperSan/ThunderPipe/issues/new) to describe your use case. Your needs may already be on the roadmap, or they could shape a future feature that benefits everyone.

## Architecture

### API calls

Clients are responsible for communicating with individual API endpoints, each exposing a method that targets a single endpoint. To understand how requests are constructed, follow the usage of `RequestBuilder` through the codebase.

### `Models/API` vs `Models/Web`

`Models/API` contains types intended for consumers of this library — wrappers around useful data that external programs work with directly.

`Models/Web` contains types used internally for API communication — what Thunderstore expects to receive and what it returns.

### Services

Services standardize multi-step API procedures. Rather than requiring every consumer to orchestrate a sequence of calls correctly, a service exposes a single method that handles the procedure end to end. This also insulates dependants from changes to Thunderstore's internals, unless the procedure itself changes significantly.

### Services vs Clients

Services are for multi-step procedures. When an operation maps to a single API call, it stays at the client level — wrapping it in a service would add complexity without benefit.
