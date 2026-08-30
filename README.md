[![](https://img.shields.io/nuget/v/Soenneker.Blob.Container.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Container/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.container/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.blob.container/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/Soenneker.Blob.Container.svg?style=for-the-badge)](https://www.nuget.org/packages/Soenneker.Blob.Container/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.blob.container/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.blob.container/actions/workflows/codeql.yml)

# Soenneker.Blob.Container

Creates missing Azure Blob containers and caches their `BlobContainerClient` instances for reuse.

## Install

```bash
dotnet add package Soenneker.Blob.Container
```

Configure the required connection string:

```json
{
  "Azure": {
    "Storage": {
      "Blob": {
        "ConnectionString": "<connection string>"
      }
    }
  }
}
```

Supply the real value through an environment-specific secret provider.

Register the singleton utility in `Program.cs`:

```csharp
using Soenneker.Blob.Container.Registrars;

builder.Services.AddBlobContainerUtilAsSingleton();
```

Singleton is the normal lifetime because the utility owns the container-client and HTTP-client caches. Scoped registration is available for specialized hosts, while its HTTP-client cache remains singleton.

## Resolve a container

```csharp
using Azure.Storage.Blobs;
using Soenneker.Blob.Container.Abstract;

BlobContainerClient invoices = await blobContainers.Get(
    "Invoices",
    cancellationToken: cancellationToken);
```

The name is normalized to lowercase, so this example targets `invoices`. The first lookup atomically creates the container if it is missing; concurrent creators do not require a separate existence check.

The returned Azure SDK client can be used directly:

```csharp
await foreach (BlobItem blob in invoices.GetBlobsAsync(
                   cancellationToken: cancellationToken))
{
    Console.WriteLine(blob.Name);
}
```

## Public access

Containers are private by default:

```csharp
BlobContainerClient assets = await blobContainers.Get(
    "public-assets",
    PublicAccessType.Blob,
    cancellationToken);
```

`publicAccessType` applies only when the container is created. It does not read, verify, or change the access policy of an existing container. Clients are cached by normalized container name, so every caller must use one consistent creation policy for that container.

Public access can expose stored data without authentication. Prefer `PublicAccessType.None` and issue narrowly scoped SAS URLs when external access is required.

## Operational behavior

- Cancellation stops pending work; it does not undo work that has already completed.
- Container creation requires credentials with the appropriate storage-account permission.
- A successful lookup is cached for the utility lifetime. External container deletion is not detected by the cache; later SDK operations will report that the container is missing.
- Dependency injection disposes the utility and its shared transport. Manually created instances must be disposed.
