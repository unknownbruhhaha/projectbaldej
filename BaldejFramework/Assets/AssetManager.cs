using System.IO;
using System.Runtime.InteropServices;

namespace BaldejFramework.Assets
{
    public static class AssetManager
    {
        public static string AssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"assets\");
        
        public static Dictionary<string, Asset> Assets = new Dictionary<string, Asset>();
        
        public static string GetAssetFullPath(string localPath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Path.Combine(AssetsPath, localPath).Replace(@"\", "/").Replace("//", "/");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(AssetsPath, localPath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(AssetsPath, localPath).Replace(@"\", "/").Replace("//", "/");
            }
            return "";
        }
    }
}
