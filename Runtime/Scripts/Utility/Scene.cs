using UnityEngine;

namespace HandyVR.Utility
{
    /// <summary>
    /// Utilities relating to Unity Scene Manipulation.
    /// </summary>
    public static class Scene
    {
        private static GameObject group;
        
        public static void BreakHierarchyAndGroup(Transform transform)
        {
            if (!group)
            {
                group = new GameObject("HandyVR");
            }
            
            transform.SetParent(group.transform);
        }

        public static void Group(Transform transform)
        {
            var t = transform;
            while (t.parent)
            {
                t = t.parent;
            }
            BreakHierarchyAndGroup(t);
        }
    }
}