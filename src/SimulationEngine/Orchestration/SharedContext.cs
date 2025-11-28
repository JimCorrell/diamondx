namespace SimulationEngine.Orchestration;

/// <summary>
/// Shared context for models to exchange data without direct coupling.
/// Thread-safe for concurrent access during parallel model execution.
/// </summary>
public interface ISharedContext
{
    /// <summary>
    /// Set a value in the shared context.
    /// </summary>
    void Set<T>(string key, T value);

    /// <summary>
    /// Get a value from the shared context.
    /// </summary>
    T Get<T>(string key);

    /// <summary>
    /// Try to get a value from the shared context.
    /// </summary>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Get a value or return the default if not found.
    /// </summary>
    T GetOrDefault<T>(string key, T defaultValue);

    /// <summary>
    /// Check if a key exists in the shared context.
    /// </summary>
    bool Contains(string key);

    /// <summary>
    /// Remove a value from the shared context.
    /// </summary>
    bool Remove(string key);

    /// <summary>
    /// Get all keys in the shared context.
    /// </summary>
    IReadOnlyList<string> GetKeys();

    /// <summary>
    /// Clear all values from the shared context.
    /// </summary>
    void Clear();

    /// <summary>
    /// Create a snapshot of the current shared context state.
    /// </summary>
    IReadOnlyDictionary<string, object?> Snapshot();
}

/// <summary>
/// Thread-safe implementation of shared context for model communication.
/// </summary>
public sealed class SharedContext : ISharedContext
{
    private readonly Dictionary<string, object?> _values = new();
    private readonly ReaderWriterLockSlim _lock = new();

    public void Set<T>(string key, T value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _lock.EnterWriteLock();
        try
        {
            _values[key] = value;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public T Get<T>(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _lock.EnterReadLock();
        try
        {
            if (!_values.TryGetValue(key, out var value))
                throw new KeyNotFoundException($"Key '{key}' not found in shared context.");

            return (T)value!;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool TryGet<T>(string key, out T? value)
    {
        ArgumentNullException.ThrowIfNull(key);
        _lock.EnterReadLock();
        try
        {
            if (_values.TryGetValue(key, out var obj))
            {
                value = (T?)obj;
                return true;
            }

            value = default;
            return false;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public T GetOrDefault<T>(string key, T defaultValue)
    {
        return TryGet<T>(key, out var value) ? value! : defaultValue;
    }

    public bool Contains(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _lock.EnterReadLock();
        try
        {
            return _values.ContainsKey(key);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool Remove(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        _lock.EnterWriteLock();
        try
        {
            return _values.Remove(key);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IReadOnlyList<string> GetKeys()
    {
        _lock.EnterReadLock();
        try
        {
            return _values.Keys.ToList();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _values.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public IReadOnlyDictionary<string, object?> Snapshot()
    {
        _lock.EnterReadLock();
        try
        {
            return new Dictionary<string, object?>(_values);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}
