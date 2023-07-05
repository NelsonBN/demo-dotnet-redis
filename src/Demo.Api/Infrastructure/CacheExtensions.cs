using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Demo.Api.Infrastructure;

public static class CacheExtensions
{
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken)
        where T : class
    {
        var bytes = await cache.GetAsync(key, cancellationToken);
        if (bytes is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public static async Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        await cache.SetAsync(
            key,
            bytes,
            options,
            cancellationToken);
    }
}