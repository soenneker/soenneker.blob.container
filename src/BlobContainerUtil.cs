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
public sealed class BlobContainerUtil : IBlobContainerUtil
{
    private const string _httpClientKey = nameof(BlobContainerUtil);

    private readonly ILogger<BlobContainerUtil> _logger;
    private readonly IHttpClientCache _httpClientCache;

    private readonly AsyncSingleton<BlobClientOptions> _blobClientOptions;
    private readonly SingletonDictionary<BlobContainerClient, PublicAccessType> _blobContainerClients;

    private readonly string _connectionString;

    public BlobContainerUtil(ILogger<BlobContainerUtil> logger, IConfiguration config, IHttpClientCache httpClientCache)
    {
        _logger = logger;
        _httpClientCache = httpClientCache;

        _connectionString = config.GetValueStrict<string>("Azure:Storage:Blob:ConnectionString");

        _blobClientOptions = new AsyncSingleton<BlobClientOptions>(CreateBlobClientOptions);
        _blobContainerClients = new SingletonDictionary<BlobContainerClient, PublicAccessType>(CreateBlobContainerClient);
    }

    private async ValueTask<BlobClientOptions> CreateBlobClientOptions(CancellationToken token)
    {
        HttpClient client = await _httpClientCache.Get(_httpClientKey, cancellationToken: token)
                                                  .NoSync();

        return new BlobClientOptions
        {
            Transport = new HttpClientTransport(client)
        };
    }

    private async ValueTask<BlobContainerClient> CreateBlobContainerClient(string containerName, CancellationToken token, PublicAccessType publicAccessType)
    {
        BlobClientOptions options = await _blobClientOptions.Get(token)
                                                            .NoSync();

        _logger.LogInformation("Connecting to Azure Blob container ({container})...", containerName);

        var containerClient = new BlobContainerClient(_connectionString, containerName, options);

        if (await containerClient.ExistsAsync(token)
                                 .NoSync())
            return containerClient;

        _logger.LogInformation("Blob container ({container}) did not exist, so creating...", containerName);

        await containerClient.CreateAsync(publicAccessType, cancellationToken: token)
                             .NoSync();

        return containerClient;
    }

    public ValueTask<BlobContainerClient> Get(string containerName, PublicAccessType publicAccessType = PublicAccessType.None,
        CancellationToken cancellationToken = default)
    {
        string containerLower = containerName.ToLowerInvariantFast();
        return _blobContainerClients.Get(containerLower, publicAccessType, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _blobContainerClients.DisposeAsync()
                                   .NoSync();
        await _blobClientOptions.DisposeAsync()
                                .NoSync();
        await _httpClientCache.Remove(_httpClientKey)
                              .NoSync();
    }

    public void Dispose()
    {
        _blobContainerClients.Dispose();
        _blobClientOptions.Dispose();
        _httpClientCache.RemoveSync(_httpClientKey);
    }
}