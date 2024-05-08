namespace GitHub_API;

public static class CacheSettings
{
    public static ushort MaxEntries { get; private set; } = 1000;
    public static TimeSpan CleanupPeriod { get; private set; } = TimeSpan.FromMinutes(30);
    public static ushort MaxEntryContributorCount { get; private set; } = 1000;
    public static bool CachingEnabled { get; private set; } = true;

}

