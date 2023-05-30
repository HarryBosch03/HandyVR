using HandyVR.Bindables;
using UnityEngine;

namespace HandyVR.Interfaces
{
    public interface IVRBindingTarget : IBehaviour
    {
        public const int HandPriority = 1;
        public const int SocketPriority = 0;
        
        public Vector3 BindingPosition { get; }
        public Quaternion BindingRotation { get; }
        public bool IsBindingFlipped { get; }
        public int BindingPriority { get; }
        
        public void OnBindingActivated(VRBinding binding);
        public void OnBindingDeactivated(VRBinding binding);
    }
}