namespace GitHub_API;

public static class DirExtension{
    public static string? ProjectBase(){
        var currDir = Directory.GetCurrentDirectory();
        var baseDir = Directory.GetParent(currDir)?.Parent?.Parent?.FullName;
        return baseDir;
    }
}