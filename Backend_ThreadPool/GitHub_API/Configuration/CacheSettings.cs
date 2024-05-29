using GitHub_API.Extensions;
using Newtonsoft.Json;

namespace GitHub_API.Configuration;

public static class CacheSettings{
    public static ushort MaxEntries { get; private set; }
    public static TimeSpan CleanupPeriod { get; private set; }
    public static ushort MaxEntryContributorCount { get; private set; }
    public static bool CachingEnabled { get; private set; }
    public static bool DoPeriodicCleanup { get; private set; }
    public static bool DoCacheTrim {  get; private set; }
    public static decimal PreTrimPct {  get; private set; }
    public static decimal PostTrimPct { get; private set; }
    public static bool AutoRestoreDefaultConfig { get; private set; }

    private static string GetConfigurationFilePath(){
        return Path.Combine(DirExtension.ProjectBase()!, 
                            Path.Combine("Configuration", 
                                         "CacheConfiguration.json"));
    }

    public static void LoadCacheSettings(){
        try{
            var fullPath = GetConfigurationFilePath();
            if (!File.Exists(fullPath)) return;
            var json = File.ReadAllText(fullPath);

            var settings = JsonConvert.DeserializeAnonymousType(json, new{
                MaxEntries = default(ushort),
                CleanupPeriod = default(TimeSpan),
                MaxEntryContributorCount = default(ushort),
                CachingEnabled = default(bool),
                DoPeriodicCleanup = default(bool),
                DoCacheTrim = default(bool),
                PreTrimPct = default(decimal),
                PostTrimPct = default(decimal),
                AutoRestoreDefaultConfig = default(bool)
            });

            if (settings == null) 
                return;
            if (settings.PreTrimPct <= 0m || settings.PostTrimPct >= 1.0m ||
                settings.PostTrimPct > settings.PreTrimPct)
                throw new ArgumentException("Wrong trim percentage");

            MaxEntries = settings.MaxEntries;
            CleanupPeriod = settings.CleanupPeriod;
            MaxEntryContributorCount = settings.MaxEntryContributorCount;
            CachingEnabled = settings.CachingEnabled;
            DoPeriodicCleanup = CachingEnabled && settings.DoPeriodicCleanup;
            DoCacheTrim =  CachingEnabled && settings.DoCacheTrim;
            PreTrimPct = settings.PreTrimPct;
            PostTrimPct = settings.PostTrimPct;
            AutoRestoreDefaultConfig = settings.AutoRestoreDefaultConfig;

        }
        
        catch(JsonSerializationException jsEx){
            Console.WriteLine($"Error loading cache settings: {jsEx.Message} | -> Restoring defaults");
            RestoreDefaultsAndLoad();
        }
        catch (OverflowException ofwEx){
            Console.WriteLine($"Error loading cache settings: {ofwEx.Message} | -> Restoring defaults");
            RestoreDefaultsAndLoad();
        }
        catch (ArgumentException argEx){
            Console.WriteLine($"Error loading cache settings: {argEx.Message} | -> Restoring defaults");
            RestoreDefaultsAndLoad();
        }
        catch (Exception ex){
            Console.WriteLine($"Error loading cache settings: {ex.Message}");
        }
        finally{
            if (AutoRestoreDefaultConfig)
                RestoreDefaultSettings();
        }
    }

    private static void RestoreDefaultsAndLoad(){
        
        RestoreDefaultSettings();
        LoadCacheSettings();
    }

    public static void RestoreDefaultSettings(){
        var fullPath = GetConfigurationFilePath();
        try{
            MaxEntries = 1000;
            CleanupPeriod = TimeSpan.FromMinutes(30);
            MaxEntryContributorCount = 1000;
            CachingEnabled = true;
            DoPeriodicCleanup = true;
            PreTrimPct = 0.8m;
            PostTrimPct = 0.4m;
            AutoRestoreDefaultConfig = true;

            string json = JsonConvert.SerializeObject(new{
                MaxEntries,
                CleanupPeriod,
                MaxEntryContributorCount,
                CachingEnabled,
                DoPeriodicCleanup,
                DoCacheTrim,
                PreTrimPct,
                PostTrimPct,
                AutoRestoreDefaultConfig
            }, Formatting.Indented);

            File.WriteAllText(fullPath, json);
        }
        catch (Exception ex){
            Console.WriteLine($"Error restoring default cache settings: {ex.Message}");
        }
    }
}

