using UnityEngine;

namespace HandyVR.Editor
{
    /// <summary>
    /// Extension methods used in HandyVR.
    /// </summary>
    internal static class Extensions
    {
        public static class NameComparisons
        {
            public delegate bool Delegate(string a, string b);

            public static string Simplify(string s) => s.Trim().ToLower().Replace(" ", "");

            public static bool Hard(string a, string b) => a == b;
            public static bool Soft(string a, string b) => Simplify(a) == Simplify(b);
        }

        public static Transform DeepFind(this Transform transform, string name, NameComparisons.Delegate areEqual = null)
        {
            areEqual ??= NameComparisons.Soft;

            if (areEqual(transform.name, name)) return transform;
            foreach (Transform child in transform)
            {
                var r = child.DeepFind(name);
                if (r) return r;
            }
            
            return null;
        }
    }
}