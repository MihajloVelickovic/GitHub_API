using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
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
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ghToken);
            
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
        return CacheSettings.DoPeriodicCleanup && 
               Cache.Count() > 0 && 
               DateTime.Now - PreviousCleanupTime >= CacheSettings.CleanupPeriod;
    }

    private static void ServeRequest(object? state){
            
        if (state == null)
            return;

        var context = (HttpListenerContext)state;

        try{
            Stopwatch stopwatch = new();
            stopwatch.Start();
            var vars = context.Request
                              .Url?
                              .Query
                              .Remove(0, 1)
                              .Split("&");

            if (vars == null)
                throw new Exception("Null query exception");
            if (vars.Length != 2)
                throw new Exception("Must have exactly 2 query parameters: \"owner\" & \"repo\"");

            var owner = vars[0].Split("=");
            var repo = vars[1].Split("=");

            if (owner[0] != "owner")
                throw new Exception("First query parameter must be the \"owner\"");
            if (repo[0] != "repo")
                throw new Exception("Second query parameter must be the \"repo\"");

            var key = $"{owner[1]}/{repo[1]}";
            
            var contributors = CacheSettings.CachingEnabled 
                             ? FetchContributorsWithCaching(ref key)
                             : FetchContributorsWithoutCaching(key);

                
            if (contributors!.Count < CacheSettings.MaxEntryContributorCount){
                CacheEntry cacheEntry = new(contributors!, DateTime.Now);
                Cache.WriteToCache(key, cacheEntry);
            }
                
            stopwatch.Stop();
                
            Console.WriteLine(key);
            var totalCommits = 0;
            foreach (var contributor in contributors){
                Console.WriteLine($"{contributor.Author!.Login}: {contributor.Total} commits");
                totalCommits += contributor.Total;
            }
            Console.WriteLine($"Total commits: {totalCommits}");
            Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalMilliseconds}ms\n");
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
        }
    }

    private static List<GitHubResult>? FetchContributorsWithCaching(ref string key){
        var contributors = Cache.Contains(key)
                         ? Cache.ReadFromCache(ref key).GitHubResult
                         : FetchContributorsWithoutCaching(key);
        
        return contributors;
    }

    private static List<GitHubResult>? FetchContributorsWithoutCaching(string key){
        var apiUrl = $"https://api.github.com/repos/{key}/stats/contributors";
        var res = HttpClient.GetAsync(apiUrl).Result;
        if (!res.IsSuccessStatusCode)
            throw new Exception($"ERROR: {res.StatusCode}");

        var content = res.Content.ReadAsStringAsync().Result;
        var contributors = JsonConvert.DeserializeObject<List<GitHubResult>>(content);       

        return contributors;
    }

    private static void CleanupCache(object? state){
        Cache.PeriodicCleanup();
    }
}
