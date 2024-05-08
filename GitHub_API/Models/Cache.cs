using GitHub_API.Configuration;

namespace GitHub_API.Models;

public static class Cache{
    private static readonly ReaderWriterLockSlim CacheLock = new();
    private static readonly Dictionary<string, CacheEntry?> CacheDict = new();

    public static bool Contains(string key){
        CacheLock.EnterReadLock();
        var test =  CacheDict.ContainsKey(key);
        CacheLock.ExitReadLock();
        return test;
    }
    
    public static CacheEntry ReadFromCache(ref string key){
        CacheLock.EnterReadLock();
        try{
            if (CacheDict.TryGetValue(key, out CacheEntry? value)){
                key += " (Result pulled from cache)";
                return value!;
            }
            else
                throw new KeyNotFoundException($"Key ({key}) not found");
        }
        catch (Exception e){
            Console.Write(e.Message);
            throw;
        }
        finally{
            CacheLock.ExitReadLock();
        }
    }

    public static int Count(){
        CacheLock.EnterWriteLock();
        var count = CacheDict.Count;
        CacheLock.ExitWriteLock();
        return count;
    }

    public static void WriteToCache(string key, CacheEntry value){
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
            if (CacheSettings.DoCacheTrim && Count() >= 0.8 * CacheSettings.MaxEntries)
                TrimCache();
        }
    }
    
    public static void PeriodicCleanup(){
        CacheLock.EnterWriteLock();
        try{
            if (Count() >= CacheSettings.MaxEntries * 0.8m)
                foreach (var kvPair in CacheDict)
                    if (DateTime.Now - kvPair.Value!.CachedTime >= CacheSettings.CleanupPeriod)
                        CacheDict.Remove(kvPair.Key);
            
        }
        catch (Exception e){
            Console.Write(e.Message);
            throw;
        }
        finally{
            CacheLock.ExitWriteLock();
        }
    }

    public static void TrimCache(){
        CacheLock.EnterWriteLock();
        try{
            if (Count() >= CacheSettings.MaxEntries * 0.8m){
                var keys = CacheDict.Keys.ToList();
                var countForRemoval = CacheSettings.MaxEntries * 0.4m;
                Random rnd = new();

                for (int i = 0; i< countForRemoval; ++i){
                    int indexToRemove = rnd.Next(0, keys.Count);
                    CacheDict.Remove(keys[indexToRemove]);
                    keys.RemoveAt(indexToRemove);
                }
            }
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
