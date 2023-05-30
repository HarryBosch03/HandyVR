using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HandyVR
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

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent(out T res) ? res : gameObject.AddComponent<T>();
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

        public static bool State(this InputAction action) => action.ReadValue<float>() > 0.5f;

        public static void SetLine(this LineRenderer lines, Vector3 a, Vector3 b, bool worldSpace = true, int subdivisions = 0)
        {
            lines.enabled = true;
            lines.positionCount = 2 + subdivisions;
            lines.useWorldSpace = worldSpace;
        
            lines.SetPosition(0, a);
            lines.SetPosition(lines.positionCount - 1, b);

            for (var i = 1; i <= subdivisions; i++)
            {
                var percent = i / (subdivisions + 1.0f);
                var vec = b - a;
                var point = a + vec * percent;
                lines.SetPosition(i, point);
            }
        }

        public static void SetRay(this LineRenderer renderer, Vector3 a, Vector3 b, bool worldSpace = true)
        {
            renderer.SetLine(a, a + b, worldSpace);
        }
        
        public static void SyncPlayState(this ParticleSystem system, bool state)
        {
            switch (system.isPlaying)
            {
                case true when state == false:
                    system.Stop();
                    break;
                case false when state == true:
                    system.Play();
                    break;
            }
        }

        public static void Template(this string template, ref string target)
        {
            target = string.Format(template, target);
        }

        public static T Ring<T>(this List<T> list, int i)
        {
            return list[(i % list.Count + list.Count) % list.Count];
        }
    }
}