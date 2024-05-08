using System.Diagnostics;
using System.Net;
using GitHub_API.Configuration;
using GitHub_API.Extensions;
using GitHub_API.Models;
using Newtonsoft.Json;

namespace GitHub_API;
public class Program{

    public static readonly HttpClient HttpClient = new();
    public static DateTime PreviousCleanupTime = DateTime.Now;

    public static void Main(string[] args){
        CacheSettings.LoadCacheSettings();
        
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null){
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
            
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "GitHub_API");
        var ghToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        //HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ghToken);
            
        var listener = new HttpListener();
        listener.Prefixes
                .Add("http://localhost:1738/");
        listener.Start();

        Console.WriteLine("Waiting for requests ....");

        while (true){
            if(RequiresPeriodicCleanup()){
                PreviousCleanupTime = DateTime.Now;
                ThreadPool.QueueUserWorkItem(CleanupCache, "Periodic cleanup");
            }

            ThreadPool.QueueUserWorkItem(ServeRequest, listener.GetContext());
        }
    }

    private static bool RequiresPeriodicCleanup(){
        return CacheSettings.DoPeriodicCleanup && Cache.Count() > 0 
            &&(DateTime.Now - PreviousCleanupTime) >= CacheSettings.CleanupPeriod;
    }

    private static void ServeRequest(object? state){
            
        if (state == null)
            return;

        var context = (HttpListenerContext)state;

        try{
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            var vars = context.Request
                              .Url?
                              .Query
                              .Remove(0, 1)
                              .Split("&");

            if (vars == null)
                throw new Exception("Null query exc");
            if (vars.Length != 2)
                throw new Exception("Mora imati tacno dva parametra");

            var owner = vars[0].Split("=");
            var repo = vars[1].Split("=");

            if (owner[0] != "owner")
                throw new Exception("Prvi argument mora biti owner");
            if (repo[0] != "repo")
                throw new Exception("Drugi argument mora biti repo");

            List<GitHubResult>? contributors = CacheSettings.CachingEnabled
                                             ? FetchContributorsWithCaching(ref owner[1], ref repo[1])
                                             : FetchContributorsWithoutCaching(ref owner[1], ref repo[1]);

            var key = $"{owner[1]}/{repo[1]}";
                
            if (contributors!.Count < CacheSettings.MaxEntryContributorCount){
                CacheEntry cacheEntry = new(contributors!, DateTime.Now);
                Cache.WriteToCache(key, cacheEntry);
            }
                
            sw1.Stop();
                
            Console.WriteLine(key);
            var totalCommits = 0;
            foreach (var contributor in contributors!){
                Console.WriteLine($"{contributor.Author!.Login}: {contributor.Total} commits");
                totalCommits += contributor.Total;
            }
            Console.WriteLine($"Total commits: {totalCommits}");
            Console.WriteLine($"Time taken: {sw1.Elapsed.TotalMilliseconds}ms\n");
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
        }
    }

    private static List<GitHubResult>? FetchContributorsWithCaching(ref string owner, ref string repo){
        List<GitHubResult>? contributors;
        var key = $"{owner}/{repo}";
        if (Cache.Contains(key))
            contributors = Cache.ReadFromCache(key).GitHubResult;

        else
            contributors = FetchContributorsWithoutCaching(ref owner, ref repo);

            return contributors;
    }

    private static List<GitHubResult>? FetchContributorsWithoutCaching(ref string owner, ref string repo)
    {
        List<GitHubResult>? contributors;
        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/stats/contributors";
        var res = HttpClient.GetAsync(apiUrl).Result;
        if (!res.IsSuccessStatusCode)
            throw new Exception($"ERROR: {res.StatusCode}");

        var content = res.Content.ReadAsStringAsync().Result;
        contributors = JsonConvert.DeserializeObject<List<GitHubResult>>(content);       

        return contributors;
    }

    private static void CleanupCache(object? state){
        Cache.PeriodicCleanup();
    }
}
