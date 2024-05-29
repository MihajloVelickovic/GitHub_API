namespace GitHub_API.Models;

public class CacheEntry: IComparable<CacheEntry>{
    public List<GitHubResult>? GitHubResult { get; set; }
    public DateTime CachedTime { get; set; } = DateTime.Now;
    public bool FromCache{ get; set; } = false;

    public CacheEntry(List<GitHubResult?>? gitHubResult, DateTime cachedTime){
        GitHubResult = gitHubResult!;
        CachedTime = cachedTime;
    }
    public CacheEntry(List<GitHubResult?>? gitHubResult){
        GitHubResult = gitHubResult!;
    }

    public int CompareTo(CacheEntry? other){
        return CachedTime > other!.CachedTime ? -1 :
               CachedTime < other!.CachedTime ? 1 : 0;
    }
}

