[![](https://img.shields.io/nuget/v/Soenneker.Blob.Container.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Container/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.container/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blob.container/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Blob.Container.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Container/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.container/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blob.container/actions/workflows/codeql.yml)

# Soenneker.Blob.Container

A utility library for Azure Blob storage container operations This should used for any connection to blob storage that we need due to it's reuse of connections. Typically Singleton IoC.

## Install

```bash
dotnet add package Soenneker.Blob.Container
```

## Quick start

```csharp
using Soenneker.Blob.Container.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddBlobContainerUtilAsSingleton();
```

Recommended.

## What you get

- `IBlobContainerUtil` — A utility library for Azure Blob storage container operations This should used for any connection to blob storage that we need due to it's reuse of connections. Typically Singleton IoC.
- `BlobContainerUtilRegistrar` — A utility library for Azure Blob storage container operations.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IBlobContainerUtil.Get(containerName, publicAccessType, cancellationToken)` | NOTE: `containerName` will be converted to lowercase. Will create container if it doesn't exist. Essentially shouldn't be used outside of other Azure Utilities. | A task whose result is the requested blob Container Client. |
| `BlobContainerUtilRegistrar.AddBlobContainerUtilAsSingleton(services)` | Recommended. | The same service collection, so additional registrations can be chained. |
| `BlobContainerUtilRegistrar.AddBlobContainerUtilAsScoped(services)` | Registers Blob Container Util with a scoped lifetime. | The same service collection, so additional registrations can be chained. |

## Practical notes

- Cancellation stops pending work; it does not undo work that has already completed.
- Calls that return a cached or singleton value reuse the same instance until the owning service is disposed.
- Dispose instances you own when their scope ends so held resources can be released.
