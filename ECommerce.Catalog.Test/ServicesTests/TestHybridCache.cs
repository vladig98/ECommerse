namespace ECommerce.Catalog.Test.ServicesTests;

internal class TestHybridCache : HybridCache
{
    public List<string> RemovedKeys { get; } = [];
    public List<string> RemovedTags { get; } = [];
    public List<string> SetKeys { get; } = [];

    public bool ThrowOnRemove { get; set; }
    public bool ThrowOnSet { get; set; }

    // This is the underlying abstract method the extension method actually calls
    public override ValueTask<T> GetOrCreateAsync<TState, T>(
        string key,
        TState state,
        Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        return factory(state, cancellationToken);
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (ThrowOnRemove)
        {
            throw new TimeoutException("Redis down");
        }

        RemovedKeys.Add(key);
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (ThrowOnRemove)
        {
            throw new TimeoutException("Redis down");
        }

        RemovedTags.Add(tag);
        return ValueTask.CompletedTask;
    }

    public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        if (ThrowOnSet)
        {
            throw new TimeoutException("Redis Timeout");
        }

        SetKeys.Add(key);
        return ValueTask.CompletedTask;
    }
}