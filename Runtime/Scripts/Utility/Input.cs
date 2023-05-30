using System;
using UnityEngine.InputSystem;

namespace HandyVR.Utility
{
    /// <summary>
    /// Utility for Unity's Input System
    /// </summary>
    public static class Input
    {
        public static void Bind(InputAction action, Action callback)
        {
            if (action == null) return;
            action.performed += _ => callback();
        }

        public static void BindAndEnable(InputAction action, Action callback)
        {
            if (action == null) return;
            Bind(action, callback);
            action.Enable();
        }
    }
}