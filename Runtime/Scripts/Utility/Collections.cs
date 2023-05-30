using System;
using System.Collections.Generic;

namespace HandyVR.Utility
{
    /// <summary>
    /// Utility relating to Collections
    /// </summary>
    public static class Collections
    {
        /// <summary>
        /// Finds the best element in a list based of a user defined scoring method.
        /// </summary>
        /// <param name="list">List to look through</param>
        /// <param name="best">The object with the largest score that is also greater than the starting score.</param>
        /// <param name="getScore">Callback used for calculating an elements score</param>
        /// <param name="startingScore">The minimum score, any element with a score less than this will be ignored.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>Whether a best was found.</returns>
        public static bool Best<T>(IEnumerable<T> list, out T best, Func<T, float> getScore, float startingScore = 0.0f)
        {
            best = default;
            var result = false;
            var bestScore = startingScore;
            foreach (var element in list)
            {
                var score = getScore(element);
                if (score < bestScore) continue;

                best = element;
                bestScore = score;
                result = true;
            }

            return result;
        }
        
        /// <summary>
        /// Finds the best element in a list based of a user defined scoring method.
        /// </summary>
        /// <param name="list">List to look through</param>
        /// <param name="getScore">Callback used for calculating an elements score</param>
        /// <param name="startingScore">The minimum score, any element with a score less than this will be ignored.</param>
        /// <typeparam name="T">The type of the elements in the list.</typeparam>
        /// <returns>The Best element in the list, if no element scored above the starting score, default is returned</returns>
        public static T Best<T>(IEnumerable<T> list, Func<T, float> getScore, float startingScore = 0.0f) where T : class
        {
            T best = null;
            var bestScore = startingScore;
            foreach (var element in list)
            {
                var score = getScore(element);
                if (score < bestScore) continue;

                best = element;
                bestScore = score;
            }

            return best;
        }
    }
}