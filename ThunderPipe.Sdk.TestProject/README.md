# ThunderPipe.Sdk.TestProject

A very scuffed test project that utilizes ThunderPipe.Sdk to test it.

Build with the following:

```sh
dotnet build ./ThunderPipe.Sdk.TestProject/ -m:1
```

This makes the build synchronous and therefore a bit slower but at least it actually uses up-to-date assemblies. Because otherwise it may have like 3 different outcomes:

1. it uses an outdated assembly
2. it uses a different outdated assembly
3. it uses an up-to-date assembly

I don't know why, I don't know how, but that just happens.
