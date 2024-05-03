namespace GitHub_API;

public static class Cache {
    private static readonly ReaderWriterLockSlim cacheLock = new();
    private static readonly Dictionary<string, List<GitHubResult>?> cache = new();

    public static bool Contains(string key){
        cacheLock.EnterReadLock();
        var test =  cache.ContainsKey(key);
        cacheLock.ExitReadLock();
        return test;
    }
    
    public static List<GitHubResult>? ReadFromCache(string key){
        cacheLock.EnterReadLock();
        try{
            if (cache.TryGetValue(key, out List<GitHubResult>? value))
                return value;
            else
                throw new KeyNotFoundException($"Kljuc ({key}) nije pronadjen");
        }
        catch (Exception e){
            Console.Write(e.Message);
            throw;
        }
        finally{
            cacheLock.ExitReadLock();
        }
    }

    public static void WriteToCache(string key, List<GitHubResult>? value){
        cacheLock.EnterWriteLock();
        try{
            cache[key] = value;
        }
        catch (Exception e){
            Console.Write(e.Message);
            throw;
        }
        finally{
            cacheLock.ExitWriteLock();
        }
    }
    public static bool WriteToCacheWithTimeout(string key, List<GitHubResult>? value, int timeout){
        if (cacheLock.TryEnterWriteLock(timeout)){
            try{
                cache[key] = value;
                return true;
            }
            catch (Exception e){
                Console.Write(e.Message);
                return false;
            }
            finally{
                cacheLock.ExitWriteLock();
            }
        }
        else{
            Console.WriteLine("Ulaz u WriteLock prevazisao timeout vreme");
            return false;
        }
    }
    public static void RemoveFromCache(string key){
        cacheLock.EnterWriteLock();
        try{
            cache.Remove(key);
        }
        catch (Exception e){
            Console.WriteLine(e.Message);
            throw;
        }
        finally{
            cacheLock.ExitWriteLock();
        }
    }
    
}
