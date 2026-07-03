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

### Clients

Clients execute a single action against the server. Each client calls the appropriate API endpoint and performs its designated task.

### Services

Services coordinate a sequence of actions to accomplish a more complex task. Rather than requiring every consumer to orchestrate a multistep procedure correctly, a service exposes a single method that handles the entire flow. This also protects consumers from changes to Thunderstore's internals, unless the underlying procedure itself changes significantly.

> [!IMPORTANT]
> Services are reserved for complex actions. When an operation maps to a single API call, it stays at the client level.

### Models

Models wrap around data, and ensure its validity. Instead of passing an arbitrary `string`, methods can pass an instance of the model, guaranteeing the intent and the validity of the data.
