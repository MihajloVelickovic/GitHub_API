﻿using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using GitHub_API.Configuration;
using GitHub_API.Extensions;
using GitHub_API.Models;
using Newtonsoft.Json;

namespace GitHub_API;
public class Program{

    public static readonly HttpClient HttpClient = new();
    public static DateTime PreviousCleanupTime = DateTime.Now;

    public static async Task Main(string[] args){
        
        CacheSettings.LoadCacheSettings();
        
        var baseDir = DirExtension.ProjectBase();
        if (baseDir != null){
            var path = Path.Combine(baseDir, ".env");
            DotEnv.Inject(path);
        }
            
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "GitHub_API");
        var ghToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        if(ghToken != null)
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ghToken);
            
        var listener = new HttpListener();
        listener.Prefixes
                .Add("http://localhost:1738/");

        try{
            listener.Start();
        }
        catch(Exception e){
            Console.Write($"Error starting listener: {e.Message}");
        }
       
        Console.WriteLine("Waiting for requests ....");

        if (CacheSettings.DoPeriodicCleanup){
            var cleanupThread = new Thread(PeriodicCleanup);
            cleanupThread.IsBackground = true;
            cleanupThread.Start();
        }
       
        while (true){
            var context = await listener.GetContextAsync();
            await Task.Run(() => ServeRequest(context));
        }
    }

    public static void PeriodicCleanup(){
        while (true){
            if (RequiresPeriodicCleanup()){
                Console.WriteLine("Performed periodic cache cleanup...");
                CleanupCache();
                PreviousCleanupTime = DateTime.Now;
            }
        }
    }
    
    private static bool RequiresPeriodicCleanup(){
        return Cache.Count() > 0 && 
               DateTime.Now - PreviousCleanupTime >= CacheSettings.CleanupPeriod;
    }

    private static async Task ServeRequest(object? state){
            
        if (state == null)
            return;

        var context = (HttpListenerContext)state;
        var response = context.Response;
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
            
            var result = CacheSettings.CachingEnabled 
                       ? await FetchContributorsWithCaching(key)
                       : await FetchContributorsWithoutCaching(key);
            
            stopwatch.Stop();
                
            if (result.FromCache)
                key += " [C]";

            var contributors = result.GitHubResult;
            
            Console.WriteLine(key);
            var totalCommits = 0;
            foreach (var contributor in contributors!){
                Console.WriteLine($"{contributor.Author!.Login}: {contributor.Total}" + 
                                  (contributor.Total > 1 ? " commits" : " commit"));
                totalCommits += contributor.Total;
            }
            Console.WriteLine($"Total commits: {totalCommits}");
            Console.WriteLine($"Time taken: {stopwatch.Elapsed.TotalMilliseconds}ms\n");
            
            var responseObject = new{
                Key = key,
                Contributors = contributors,
                TotalCommits = totalCommits,
                TotalTime = stopwatch.Elapsed.TotalMilliseconds
            };
            var responseJson = JsonConvert.SerializeObject(responseObject);
            var responseByteArray = Encoding.UTF8.GetBytes(responseJson);
            response.ContentLength64 = responseByteArray.Length;
            response.ContentType = "text/json";
            await response.OutputStream.WriteAsync(responseByteArray);
        }
        catch(JsonException){
            Console.WriteLine("API error: Returned empty json");
            await SendErrorResponse(response, "API error: Returned empty JSON", HttpStatusCode.BadRequest);
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
            await SendErrorResponse(response, e.Message, HttpStatusCode.InternalServerError);
        }
    }

    private static async Task<CacheEntry> FetchContributorsWithCaching(string key){
        var result = Cache.Contains(key)
                   ? Cache.ReadFromCache(key)
                   : await FetchContributorsWithoutCaching(key);
        
        return result;
    }
    
    private static async Task<CacheEntry> FetchContributorsWithoutCaching(string key){
        var apiUrl = $"https://api.github.com/repos/{key}/stats/contributors";
        var res = await HttpClient.GetAsync(apiUrl);
        if (!res.IsSuccessStatusCode)
            throw new Exception($"API ERROR: {res.StatusCode}");

        var content = await res.Content.ReadAsStringAsync();
        var contributors = JsonConvert.DeserializeObject<List<GitHubResult>>(content);       

        if (CacheSettings.CachingEnabled && contributors!.Count < CacheSettings.MaxEntryContributorCount){
            CacheEntry cacheEntry = new(contributors!, DateTime.Now);
            Cache.WriteToCache(key, cacheEntry);
            return cacheEntry;
        }
        return new CacheEntry(contributors!, DateTime.Now);
    }

    private static async Task SendErrorResponse(HttpListenerResponse response, string message, HttpStatusCode statusCode)
    {
        try{
            if (message == null)
                throw new ArgumentNullException("The message was null");

            var errorResponse = new
            {
                StatusCode = (int)statusCode,
                Message = message
            };
            var responseJson = JsonConvert.SerializeObject(errorResponse);
            var responseByteArray = Encoding.UTF8.GetBytes(responseJson);
            response.ContentLength64 = responseByteArray.Length;
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;
            await response.OutputStream.WriteAsync(responseByteArray);
        }
        catch(ArgumentNullException e){
            Console.WriteLine($"Null argument exception: {e.Message}");
        }
        catch(Exception e){
            Console.WriteLine($"Error sending error response: {e.Message}");
        }
    }

    private static void CleanupCache(){
        Cache.PeriodicCleanup();
    }
}
