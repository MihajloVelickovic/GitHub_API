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
    
    public static CacheEntry ReadFromCache(string key){
        CacheLock.EnterReadLock();
        try{
            if (CacheDict.TryGetValue(key, out CacheEntry? value)){
                value!.CachedTime = DateTime.Now;
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
            if (CheckCacheTrim())
                TrimCache();
        }
    }
    
    public static void PeriodicCleanup(){
        CacheLock.EnterWriteLock();
        try{
            if (Count() >= CacheSettings.MaxEntries * CacheSettings.PreTrimPct)
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
            if (Count() >= CacheSettings.MaxEntries * CacheSettings.PreTrimPct){
                var forDeletion = CacheDict.ToList();
                var countForRemoval = CacheSettings.MaxEntries * CacheSettings.PostTrimPct;
                forDeletion.Sort((pair1, pair2) => pair1.Value!.CompareTo(pair2.Value));

                for(int i=0; i<countForRemoval; ++i)
                    CacheDict.Remove(forDeletion[i].Key);
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

    public static int Count(){
        CacheLock.EnterWriteLock();
        var count = CacheDict.Count;
        CacheLock.ExitWriteLock();
        return count;
    }

    private static bool CheckCacheTrim(){
        return CacheSettings.DoCacheTrim &&
                Count() >= CacheSettings.PreTrimPct * CacheSettings.MaxEntries;
    }
}
