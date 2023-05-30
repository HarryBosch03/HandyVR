using UnityEngine;

namespace HandyVR.Utility
{
    /// <summary>
    /// Utility for Physics.
    /// </summary>
    public static class Physics
    {
        /// <summary>
        /// Tells the Physics Engine Whether two objects should ignore each others collisions.
        /// Applied to all children of both objects.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="ignore"></param>
        public static void IgnoreCollision(GameObject a, GameObject b, bool ignore)
        {
            var acl = a.GetComponentsInChildren<Collider>(true);
            var bcl = b.GetComponentsInChildren<Collider>(true);

            foreach (var ac in acl)
            foreach (var bc in bcl)
            {
                UnityEngine.Physics.IgnoreCollision(ac, bc, ignore);
            }
        }
    }
}