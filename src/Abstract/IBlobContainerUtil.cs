using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Soenneker.Blob.Container.Abstract;

/// <summary>
/// A utility library for Azure Blob storage container operations <para/>
/// This should used for any connection to blob storage that we need due to it's reuse of connections. <para/>
/// Typically Singleton IoC
/// </summary>
public interface IBlobContainerUtil : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// NOTE: <paramref name="containerName"/> will be converted to lowercase. Will create container if it doesn't exist. Essentially shouldn't be used outside of
    /// other Azure Utilities
    /// </summary>
    [Pure]
    ValueTask<BlobContainerClient> Get(string containerName, PublicAccessType publicAccessType = PublicAccessType.None);
}