using System;
using System.Net.Http;
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
    private readonly ILogger<BlobContainerUtil> _logger;
    private readonly IHttpClientCache _httpClientCache;

    private readonly AsyncSingleton<BlobClientOptions> _blobClientOptions;
    private readonly SingletonDictionary<BlobContainerClient> _blobContainerClients;

    public BlobContainerUtil(ILogger<BlobContainerUtil> logger, IConfiguration config, IHttpClientCache httpClientCache)
    {
        _logger = logger;
        _httpClientCache = httpClientCache;

        _blobClientOptions = new AsyncSingleton<BlobClientOptions>(async () =>
        {
            HttpClient client = await httpClientCache.Get(nameof(BlobContainerUtil)).NoSync();

            var blobClientOptions = new BlobClientOptions
            {
                Transport = new HttpClientTransport(client)
            };
            return blobClientOptions;
        });

        _blobContainerClients = new SingletonDictionary<BlobContainerClient>(async args =>
        {
            BlobClientOptions options = await _blobClientOptions.Get().NoSync();

            var containerName = (string)args![0];
            var publicAccessType = (PublicAccessType)args[1];

            var connectionString = config.GetValueStrict<string>("Azure:Storage:Blob:ConnectionString");

            _logger.LogInformation("Connecting to Azure Blob container ({container})...", containerName);

            var containerClient = new BlobContainerClient(connectionString, containerName, options);

            if (await containerClient.ExistsAsync().NoSync())
                return containerClient;

            _logger.LogInformation("Blob container ({container}) did not exist, so creating...", containerName);
            await containerClient.CreateAsync(publicAccessType).NoSync();

            return containerClient;
        });
    }

    public ValueTask<BlobContainerClient> Get(string containerName, PublicAccessType publicAccessType = PublicAccessType.None)
    {
        string containerLower = containerName.ToLowerInvariantFast();

        return _blobContainerClients.Get(containerLower, containerLower, publicAccessType);
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
        _httpClientCache.Remove(nameof(BlobContainerUtil));
    }
}