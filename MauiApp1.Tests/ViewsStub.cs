namespace MauiApp1.Views 
{ 
    public class AssetDetailPage { } 
}

namespace Microsoft.Maui.Storage
{
    public static class Preferences
    {
        public static string Get(string key, string defaultValue) => defaultValue;
        public static void Set(string key, string value) { }
    }
}

namespace Microsoft.Maui.Controls
{
    public static class Shell
    {
        public static object? Current => null;
    }
}
