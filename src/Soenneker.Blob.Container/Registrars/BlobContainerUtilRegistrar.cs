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
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddBlobContainerUtilAsSingleton(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddSingleton<IBlobContainerUtil, BlobContainerUtil>();

        return services;
    }

    /// <summary>
    /// Registers Blob Container Util with a scoped lifetime.
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddBlobContainerUtilAsScoped(this IServiceCollection services)
    {
        services.AddHttpClientCacheAsSingleton().TryAddScoped<IBlobContainerUtil, BlobContainerUtil>();

        return services;
    }
}
