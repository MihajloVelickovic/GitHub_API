namespace GitHub_API;

public static class Cache {
    private static readonly ReaderWriterLockSlim CacheLock = new();
    private static readonly Dictionary<string, List<GitHubResult>?> CacheDict = new();

    public static bool Contains(string key){
        CacheLock.EnterReadLock();
        var test =  CacheDict.ContainsKey(key);
        CacheLock.ExitReadLock();
        return test;
    }
    
    public static List<GitHubResult>? ReadFromCache(string key){
        CacheLock.EnterReadLock();
        try{
            if (CacheDict.TryGetValue(key, out List<GitHubResult>? value))
                return value;
            else
                throw new KeyNotFoundException($"Kljuc ({key}) nije pronadjen");
        }
        catch (Exception e){
            Console.Write(e.Message);
            throw;
        }
        finally{
            CacheLock.ExitReadLock();
        }
    }

    public static void WriteToCache(string key, List<GitHubResult>? value){
        CacheLock.EnterWriteLock();
        try{
            CacheDict[key] = value;
        }
        catch (Exception e){
            Console.Write(e.Message);
            throw;
        }
        finally{
            CacheLock.ExitWriteLock();
        }
    }
    
}
