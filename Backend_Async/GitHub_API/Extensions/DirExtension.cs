namespace GitHub_API.Extensions;

public static class DirExtension
{
    public static string? ProjectBase()
    {
        var currDir = Directory.GetCurrentDirectory();
        var baseDir = Directory.GetParent(currDir)?.Parent?.Parent?.FullName;
        return baseDir;
    }
}