// Avishai Dernis 2025

using System.Threading.Tasks;

namespace Zarem.Services;

/// <summary>
/// An interface for a service that caches values.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Caches a value.
    /// </summary>
    Task CacheAsync<T>(string key, T value) where T : class, new();

    /// <summary>
    /// Caches a value.
    /// </summary>
    Task CacheAsync(string key, string? value);

    /// <summary>
    /// Retrieves a cached value.
    /// </summary>
    Task<T?> RetrieveCacheAsync<T>(string key) where T : class, new();

    /// <summary>
    /// Retrieves a cached value.
    /// </summary>
    Task<string?> RetrieveCacheAsync(string key);

    /// <summary>
    /// Deletes a cached value.
    /// </summary>
    Task DeleteCacheAsync(string key);
}
