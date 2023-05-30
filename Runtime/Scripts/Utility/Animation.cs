using System;
using UnityEngine;

namespace HandyVR.Utility
{
    public static class Animation
    {
        public delegate void AnimationDefinition<T>(ref T current, T target, float speed);
        
        public static void SimpleMoveTowards(ref float current, float target, float speed) => current += (target - current) / speed * Time.deltaTime;
        public static void SimpleMoveTowards(ref Vector3 current, Vector3 target, float speed) => current += (target - current) / speed * Time.deltaTime;

        public static T WithoutRef<T>(T current, T target, float speed, AnimationDefinition<T> animation)
        {
            var val = current;
            animation(ref val, target, speed);
            return val;
        }
    }
}