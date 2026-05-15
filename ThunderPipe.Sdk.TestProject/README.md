# ThunderPipe.Sdk.TestProject

A very scuffed test project that utilizes `ThunderPipe.Sdk` to test it.

## Build

Build the project with the following:

```sh
dotnet build ./ThunderPipe.Sdk.TestProject/ -m:1
```

This makes the build synchronous and therefore a bit slower but at least it actually uses up-to-date assemblies. Otherwise, it may have like 3 different outcomes:

1. It uses an outdated assembly
2. It uses a different outdated assembly
3. It uses an up-to-date assembly

This makes the build sequential, which ensures projects build in the correct order. Parallel builds can pick up stale assemblies from a previous build.

## Test SDK

To test the SDK, you need to restore the dependencies of this project. It will clear the cache and rebuild `ThunderPipe.Sdk`.
