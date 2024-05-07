namespace GitHub_API;

public static class CacheSettings
{
    public static uint MaxEntries { get; private set; } = 1000;
    public static TimeSpan CleanupPeriod { get; private set; } = TimeSpan.FromMinutes(30);
    public static uint MaxEntryContributorCount { get; private set; } = 1000;
    public static bool CachingEnabled { get; private set; } = true;

}

