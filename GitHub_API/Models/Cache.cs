namespace GitHub_API;

public static class Cache {
    private static readonly ReaderWriterLockSlim CacheLock = new();
    private static readonly Dictionary<string,CacheEntry?> CacheDict = new();

    public static bool Contains(string key){
        CacheLock.EnterReadLock();
        var test =  CacheDict.ContainsKey(key);
        CacheLock.ExitReadLock();
        return test;
    }
    
    public static CacheEntry ReadFromCache(string key){
        CacheLock.EnterReadLock();
        try{
            if (CacheDict.TryGetValue(key, out CacheEntry value))
                return value!;
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

    public static int Count(){
        CacheLock.EnterWriteLock();
        var count = CacheDict.Count();
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

            if (Count() > CacheSettings.MaxEntries * 0.8)
                CacheCleanup();
        }
    }
    
    public static void PeriodicCleanup(){
        CacheLock.EnterWriteLock();
        try{
            if (Count() >= CacheSettings.MaxEntries * 0.8m){
                List<string> keysToRemove = new();
                foreach (var kvPair in CacheDict)
                    if (DateTime.Now - kvPair.Value!.CachedTime >= CacheSettings.CleanupPeriod)
                        keysToRemove.Add(kvPair.Key);

                foreach (var key in keysToRemove)
                    CacheDict.Remove(key);
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

    public static void CacheCleanup(){
        CacheLock.EnterWriteLock();
        try{
            List<string> keys = CacheDict.Keys.ToList();
            decimal countForRemoval = CacheSettings.MaxEntries * 0.4m;
            Random rnd = new Random();

            for(int i=0; i< countForRemoval;++i){
                int indexToRemove = rnd.Next(0, keys.Count);
                CacheDict.Remove(keys[indexToRemove]);
                keys.RemoveAt(indexToRemove);
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
