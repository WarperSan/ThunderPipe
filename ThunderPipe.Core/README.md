<p align="center">
    <img src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/refs/heads/master/icon.png" alt="Logo" height="128"/>
</p>

# ThunderPipe.Core
[![ThunderPipe](https://img.shields.io/badge/ThunderPipe-blue)](https://github.com/WarperSan/ThunderPipe)
[![NuGet Badge](https://img.shields.io/nuget/v/ThunderPipe.Core)](https://www.nuget.org/packages/ThunderPipe.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ThunderPipe.Core?color=purple)](https://www.nuget.org/packages/ThunderPipe.Core)
[![License](https://img.shields.io/github/license/WarperSan/ThunderPipe)](https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE)
[![Codecov Badge](https://codecov.io/github/WarperSan/ThunderPipe/graph/badge.svg)](https://codecov.io/github/WarperSan/ThunderPipe)

ThunderPipe.Core is the core logic of every ThunderPipe product.

## Custom Product

If the needs are not met with the provided products, it is possible to create custom ones. They simply need to reference this package. They will have access to everything ThunderPipe.Core has to offer.

## Development

This section goes over how ThunderPipe.Core is made.

### Where are the API calls?

Clients are responsible to communicate with API endpoints. They offer simple methods that addresses a single endpoint.

You can see how requests are made by following how the `RequestBuilder` is used.

### Why `Models/API` and `Models/Web`?

Classes in `Models/API` are intended for other program's use. They are wrappers around useful data types.

On the other hand, `Models/Web` is meant for internal API calls. They are what Thunderstore expects and returns.

### What are Services?

Services are a way to standardize a series of API calls. Instead of requiring every other program to orchestrate the procedure correctly, ThunderPipe.Core offers a single method that does it.

This also removes the weak link between other programs and Thunderstore. Unless the procedure changes drastically, the dependants will not have to change their calls.

### Why a Service and not a Client?

Services are meant for a series of calls. When looking at certain call, they only require one method. Theses will stay at the client level, as they are too simple for services.
