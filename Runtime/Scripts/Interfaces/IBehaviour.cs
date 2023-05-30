using UnityEngine;
// ReSharper disable InconsistentNaming

namespace HandyVR.Interfaces
{
    public interface IBehaviour
    {
        public GameObject gameObject { get; }
        
        public Transform transform { get; }

    }
}