using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Blob.Container.Abstract;
using Soenneker.Utils.HttpClientCache.Registrar;

namespace Soenneker.Blob.Container.Registrars;

/// <summary>
/// A utility library for Azure Blob storage container operations
/// </summary>
public static class BlobContainerUtilRegistrar
{
    /// <summary>
    /// Recommended
    /// </summary>
    public static void AddBlobContainerUtilAsSingleton(this IServiceCollection services)
    {
        services.AddHttpClientCache();
        services.TryAddSingleton<IBlobContainerUtil, BlobContainerUtil>();
    }

    public static void AddBlobContainerUtilAsScoped(this IServiceCollection services)
    {
        services.AddHttpClientCache();
        services.TryAddScoped<IBlobContainerUtil, BlobContainerUtil>();
    }
}
