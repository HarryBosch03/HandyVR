using System.Collections.Generic;
using UnityEditor;
using Object = UnityEngine.Object;

namespace HandyVR.Editor
{
    public static class Utility
    {
        public delegate bool PathFilter(string path);

        public static bool DefaultFilter(string path) => path[..7] == "Assets/";

        public static List<T> Find<T>(PathFilter filter = null) where T : Object => Find<T>($"t:{nameof(T)}");
        public static List<T> Find<T>(string pattern, PathFilter filter = null) where T : Object
        {
            filter ??= DefaultFilter;
            
            var guids = AssetDatabase.FindAssets(pattern);
            var assets = new List<T>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!filter(path)) continue;
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (!asset) continue;
                assets.Add(asset);
            }

            return assets;
        }
    }
}