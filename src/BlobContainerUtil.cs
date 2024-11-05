using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Soenneker.Blob.Container.Abstract;
using Soenneker.Extensions.Configuration;
using Soenneker.Extensions.String;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Utils.AsyncSingleton;
using Soenneker.Utils.HttpClientCache.Abstract;
using Soenneker.Utils.SingletonDictionary;

namespace Soenneker.Blob.Container;

///<inheritdoc cref="IBlobContainerUtil"/>
public class BlobContainerUtil : IBlobContainerUtil
{
    private readonly IHttpClientCache _httpClientCache;

    private readonly AsyncSingleton<BlobClientOptions> _blobClientOptions;
    private readonly SingletonDictionary<BlobContainerClient> _blobContainerClients;

    public BlobContainerUtil(ILogger<BlobContainerUtil> logger, IConfiguration config, IHttpClientCache httpClientCache)
    {
        _httpClientCache = httpClientCache;

        // Keep as singleton, the dictionary references it multiple times; we want to share it
        _blobClientOptions = new AsyncSingleton<BlobClientOptions>(async (token, _) =>
        {
            HttpClient client = await httpClientCache.Get(nameof(BlobContainerUtil), cancellationToken: token).NoSync();

            var blobClientOptions = new BlobClientOptions
            {
                Transport = new HttpClientTransport(client)
            };

            return blobClientOptions;
        });

        _blobContainerClients = new SingletonDictionary<BlobContainerClient>(async (containerName, token, args) =>
        {
            BlobClientOptions options = await _blobClientOptions.Get(token).NoSync();

            var publicAccessType = (PublicAccessType)args[0];

            var connectionString = config.GetValueStrict<string>("Azure:Storage:Blob:ConnectionString");

            logger.LogInformation("Connecting to Azure Blob container ({container})...", containerName);

            var containerClient = new BlobContainerClient(connectionString, containerName, options);

            if (await containerClient.ExistsAsync(token).NoSync())
                return containerClient;

            logger.LogInformation("Blob container ({container}) did not exist, so creating...", containerName);
            await containerClient.CreateAsync(publicAccessType, cancellationToken: token).NoSync();

            return containerClient;
        });
    }

    public ValueTask<BlobContainerClient> Get(string containerName, PublicAccessType publicAccessType = PublicAccessType.None, CancellationToken cancellationToken = default)
    {
        string containerLower = containerName.ToLowerInvariantFast();

        return _blobContainerClients.Get(containerLower, cancellationToken, publicAccessType);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await _blobContainerClients.DisposeAsync().NoSync();
        await _blobClientOptions.DisposeAsync().NoSync();
        await _httpClientCache.Remove(nameof(BlobContainerUtil)).NoSync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _blobContainerClients.Dispose();
        _blobClientOptions.Dispose();
        _httpClientCache.RemoveSync(nameof(BlobContainerUtil));
    }
}