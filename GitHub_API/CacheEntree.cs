using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitHub_API;

public class CacheEntree
{
    public List<GitHubResult>? GitHubResult { get; set; }
    public DateTime CachedTime { get; set; } = DateTime.Now;

    public CacheEntree(List<GitHubResult?>? gitHubResult, DateTime cachedTime){
        GitHubResult = gitHubResult!;
        CachedTime = cachedTime;
    }   
}

