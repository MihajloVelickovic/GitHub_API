namespace GitHub_API.Models;

public class CacheEntry
{
    public List<GitHubResult>? GitHubResult { get; set; }
    public DateTime CachedTime { get; set; } = DateTime.Now;

    public CacheEntry(List<GitHubResult?>? gitHubResult, DateTime cachedTime)
    {
        GitHubResult = gitHubResult!;
        CachedTime = cachedTime;
    }
}

