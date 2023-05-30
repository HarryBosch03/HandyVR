using System.Collections.Generic;
using HandyVR.Bindables;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;

namespace HandyVR.Interfaces
{
    public interface IVRBindable : IBehaviour
    {
        VRBinding ActiveBinding { get; }
        Rigidbody Rigidbody { get; }
        
        void OnBindingActivated(VRBinding newBinding);
        void OnBindingDeactivated(VRBinding oldBinding);
        void InputCallback(VRHand hand, InputType type, HandInput.InputWrapper input);

        bool IsValid();

        public static readonly List<IVRBindable> All = new();
        
        public static bool Valid(IVRBindable ivrBindable)
        {
            return ivrBindable != null && ivrBindable.IsValid();
        }
        
        public enum InputType
        {
            Trigger,
        }
    }
}