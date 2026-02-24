// Avishai Dernis 2025

using CommunityToolkit.Mvvm.Messaging;
using Zarem.Messages;
using Zarem.Services;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Zarem.IDE.Services;

/// <summary>
/// An implementation of the <see cref="ICacheService"/>.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMessenger _messenger;
    private readonly AsyncMutex _mutex;
    private readonly StorageFolder _folder;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    public CacheService(IMessenger messenger)
    {
        _messenger = messenger;

        _mutex = new AsyncMutex();
        _folder = ApplicationData.Current.LocalCacheFolder;
    }

    /// <inheritdoc/>
    public async Task CacheAsync(string key, string? value)
    {
        using (await _mutex.LockAsync())
        {
            // Save cache
            var file = await _folder.CreateFileAsync(key, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(file, value);
        }
    }

    /// <inheritdoc/>
    public async Task CacheAsync<T>(string key, T value)
        where T : class, new()
    {
        var serialized = JsonSerializer.Serialize(value);
        await CacheAsync(key, serialized);
        _messenger.Send(new CacheChangedMessage<T>(key, value));
    }

    /// <inheritdoc/>
    public async Task<string?> RetrieveCacheAsync(string key)
    {
        using (await _mutex.LockAsync())
        {
            // Retrieve file
            var item = await _folder.TryGetItemAsync(key);
            if (item is not IStorageFile file)
                return null;

            // Retrieve value
            return await FileIO.ReadTextAsync(file);
        }
    }

    /// <inheritdoc/>
    public async Task<T?> RetrieveCacheAsync<T>(string key)
        where T : class, new()
    {
        var json = await RetrieveCacheAsync(key);
        if (json is null)
            return null;

        return JsonSerializer.Deserialize<T>(json);
    }

    /// <inheritdoc/>
    public async Task DeleteCacheAsync(string key)
    {
        using (await _mutex.LockAsync())
        {
            // Delete file
            var item = await _folder.TryGetItemAsync(key);
            if (item is IStorageFile file)
                await file.DeleteAsync();
        }
    }

    private sealed class AsyncMutex
    {
        // Stolen directly from Legere

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public async Task<IDisposable> LockAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            return new Lock(_semaphore);
        }

        private sealed partial class Lock(SemaphoreSlim semaphore) : IDisposable
        {
            private readonly SemaphoreSlim _semaphore = semaphore;

            public void Dispose() => _semaphore.Release();
        }
    }
}
