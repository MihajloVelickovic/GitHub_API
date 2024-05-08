using GitHub_API.Extensions;
using Newtonsoft.Json;

namespace GitHub_API.Configuration;

public static class CacheSettings
{
    public static ushort MaxEntries { get; private set; }
    public static TimeSpan CleanupPeriod { get; private set; }
    public static ushort MaxEntryContributorCount { get; private set; }
    public static bool CachingEnabled { get; private set; }
    public static bool DoPeriodicCleanup { get; private set; }
    public static bool DoCacheTrim {  get; private set; }
    public static bool AutoRestoreDefaultConfig { get; private set; }

    private static string GetConfigurationFilePath()
    {
        return Path.Combine(DirExtension.ProjectBase()!,
                                        "Configuration\\CacheConfiguration.json");
    }

    public static void LoadCacheSettings()
    {
        try
        {
            var fullPath = GetConfigurationFilePath();
            if (File.Exists(fullPath))
            {
                string json = File.ReadAllText(fullPath);

                var settings = JsonConvert.DeserializeAnonymousType(json, new
                {
                    MaxEntries = default(ushort),
                    CleanupPeriod = default(TimeSpan),
                    MaxEntryContributorCount = default(ushort),
                    CachingEnabled = default(bool),
                    DoPeriodicCleanup = default(bool),
                    DoCacheTrim = default(bool),
                    AutoRestoreDefaultConfig = default(bool)
                });

                if (settings != null)
                {
                    MaxEntries = settings.MaxEntries;
                    CleanupPeriod = settings.CleanupPeriod;
                    MaxEntryContributorCount = settings.MaxEntryContributorCount;
                    CachingEnabled = settings.CachingEnabled;
                    DoPeriodicCleanup = CachingEnabled ? settings.DoPeriodicCleanup : false;
                    DoCacheTrim =  CachingEnabled ? settings.DoCacheTrim : false;
                    AutoRestoreDefaultConfig = settings.AutoRestoreDefaultConfig;
                }
            }

        }
        catch(JsonSerializationException jsEX){
            Console.WriteLine($"Error loading cache settings: {jsEX.Message} | -> Restoring defaults");
            RestoreDefaultsAndLoad();
        }
        catch (OverflowException ofwEX){
            Console.WriteLine($"Error loading cache settings: {ofwEX.Message} | -> Restoring defaults");
            RestoreDefaultsAndLoad();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cache settings: {ex.Message}");
        }
        finally
        {
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
            AutoRestoreDefaultConfig = true;

            string json = JsonConvert.SerializeObject(new
            {
                MaxEntries,
                CleanupPeriod,
                MaxEntryContributorCount,
                CachingEnabled,
                DoPeriodicCleanup,
                DoCacheTrim,
                AutoRestoreDefaultConfig
            }, Formatting.Indented);

            File.WriteAllText(fullPath, json);
        }
        catch (Exception ex){
            Console.WriteLine($"Error restoring default cache settings: {ex.Message}");
        }
    }
}

