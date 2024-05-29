namespace GitHub_API.Models;

public class CacheEntry: IComparable<CacheEntry>{
    public List<GitHubResult>? GitHubResult { get; set; }
    public DateTime CachedTime { get; set; } = DateTime.Now;
    public CacheEntry(List<GitHubResult?>? gitHubResult, DateTime cachedTime)
    {
        GitHubResult = gitHubResult!;
        CachedTime = cachedTime;
    }

    public int CompareTo(CacheEntry? other){
        return CachedTime > other!.CachedTime ? -1 :
               CachedTime < other!.CachedTime ? 1 : 0;
    }
}

