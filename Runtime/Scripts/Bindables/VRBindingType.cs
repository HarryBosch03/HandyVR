using HandyVR.Bindables.Pickups;
using HandyVR.Bindables.Targets;
using UnityEngine;

namespace HandyVR.Bindables
{
    /// <summary>
    /// Asset used to mask what <see cref="VRPickup">Pickups</see> can go in <see cref="VRSocket">sockets</see>.
    /// </summary>
    [CreateAssetMenu(menuName = "HandyVR/VR Binding Type")]
    public class VRBindingType : ScriptableObject
    {
        [SerializeField] private bool debug;

        public bool Debug => debug;
    }
}