using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Soenneker.Blob.Container.Abstract;

/// <summary>
/// Resolves and caches Azure Blob container clients, creating missing containers when first requested.
/// </summary>
public interface IBlobContainerUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets a cached container client and creates the container atomically if it does not exist.
    /// </summary>
    /// <param name="containerName">Name of the container to target.</param>
    /// <param name="publicAccessType">Public access level to use only if the container must be created.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The client targeting the normalized container name.</returns>
    ValueTask<BlobContainerClient> Get(string containerName, PublicAccessType publicAccessType = PublicAccessType.None, CancellationToken cancellationToken = default);
}
