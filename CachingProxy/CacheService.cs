public static class CacheService
{
    private static Dictionary<string, CachedItem> _cache = new();

    public static bool TryGet(string path, out CachedItem item)
    {
        return _cache.TryGetValue(path, out item);
    }

    public static void Set(string path, CachedItem item)
    {
        _cache[path] = item;
    }

    public static void Clear()
    {
        _cache.Clear();
    }
}