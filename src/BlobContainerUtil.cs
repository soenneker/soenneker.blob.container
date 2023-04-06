using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using Soenneker.Blob.Container.Abstract;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Blob.Container;

///<inheritdoc cref="IBlobContainerUtil"/>
public class BlobContainerUtil : IBlobContainerUtil
{
    // Does not need disposal: https://github.com/Azure/azure-sdk-for-net/issues/7082
    private readonly ConcurrentDictionary<string, BlobContainerClient> _blobContainerCache;

    private readonly ILogger<BlobContainerUtil> _logger;

    private readonly AsyncLock _containerLock;

    private readonly AsyncSingleton<BlobClientOptions> _blobClientOptions;
    private readonly AsyncSingleton<HttpClient> _httpClient;
    private readonly IConfiguration _config;

    public BlobContainerUtil(ILogger<BlobContainerUtil> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        _containerLock = new AsyncLock();
        _blobContainerCache = new ConcurrentDictionary<string, BlobContainerClient>();

        _httpClient = new AsyncSingleton<HttpClient>(() =>
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(10),
                MaxConnectionsPerServer = 50
            };

            var httpClient = new HttpClient(socketsHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(120); // TODO: Review blob download timeout

            return httpClient;
        });

        _blobClientOptions = new AsyncSingleton<BlobClientOptions>(async () =>
        {
            HttpClient client = await _httpClient.Get();

            var blobClientOptions = new BlobClientOptions
            {
                Transport = new HttpClientTransport(client)
            };
            return blobClientOptions;
        });
    }

    public async ValueTask<BlobContainerClient> GetClient(string containerName, PublicAccessType publicAccessType = PublicAccessType.None)
    {
        string containerLower = containerName.ToLowerInvariant();

        if (_blobContainerCache.TryGetValue(containerLower, out BlobContainerClient? containerClient))
            return containerClient;

        using (await _containerLock.LockAsync())
        {
            if (_blobContainerCache.TryGetValue(containerLower, out containerClient))
                return containerClient;

            containerClient = await InitContainer(containerLower, publicAccessType);

            _blobContainerCache.TryAdd(containerLower, containerClient);
        }

        return containerClient;
    }

    /// <summary>
    /// We have not found a client in cache, so lets create a new one
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    private async ValueTask<BlobContainerClient> InitContainer(string containerName, PublicAccessType accessType = PublicAccessType.None)
    {
        BlobClientOptions options = await _blobClientOptions.Get();

        var connectionString = _config.GetValue<string>("Azure:Storage:Blob:ConnectionString");

        if (connectionString == null)
            throw new NullReferenceException( "Azure:Storage:Blob:ConnectionString is required");

        var containerClient = new BlobContainerClient(connectionString, containerName, options);

        if (await containerClient.ExistsAsync())
            return containerClient;

        _logger.LogInformation("Container did not exist, so creating: {name} ...", containerName);
        await containerClient.CreateAsync(accessType);

        return containerClient;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        await _httpClient.DisposeAsync();
        await _blobClientOptions.DisposeAsync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _httpClient.Dispose();
        _blobClientOptions.Dispose();
    }
}