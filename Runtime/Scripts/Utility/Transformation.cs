namespace HandyVR.Utility
{
    /// <summary>
    /// Utilities relating to transformations.
    /// </summary>
    public static class Transformation
    {
        /// <summary>
        /// Finds the difference between two rotations.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static UnityEngine.Quaternion Difference(UnityEngine.Quaternion a, UnityEngine.Quaternion b)
        {
            var a2 = UnityEngine.Quaternion.identity * UnityEngine.Quaternion.Inverse(a);
            var b2 = UnityEngine.Quaternion.identity * UnityEngine.Quaternion.Inverse(b);

            return b2 * UnityEngine.Quaternion.Inverse(a2);
        }
    }
}