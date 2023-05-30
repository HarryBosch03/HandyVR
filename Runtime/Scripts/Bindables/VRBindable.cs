using HandyVR.Bindables.Pickups;
using HandyVR.Interfaces;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;

namespace HandyVR.Bindables
{
    /// <summary>
    /// The base for objects that can be bound, either via hands, sockets, etc...
    /// </summary>
    public abstract class VRBindable : MonoBehaviour, IVRBindable
    {
        public VRBinding ActiveBinding { get; private set; }
        public abstract Rigidbody Rigidbody { get; }

        public Vector3 BindingPosition => ActiveBinding.target.BindingPosition;
        public Quaternion BindingRotation => ActiveBinding.target.BindingRotation;
        public bool BindingFlipped => ActiveBinding.target.IsBindingFlipped;
        
        public bool IsValid() => this;

        protected virtual void OnEnable()
        {
            IVRBindable.All.Add(this);
        }

        protected virtual void OnDisable()
        {
            IVRBindable.All.Remove(this);
        }

        public virtual void OnBindingActivated(VRBinding binding)
        {
            ActiveBinding = binding;
        }
        public virtual void OnBindingDeactivated(VRBinding binding) { }

        /// <summary>
        /// Used to find pickups within a range.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static IVRBindable GetBindable(Vector3 from, float range)
        {
            IVRBindable res = null;
            foreach (var pickup in IVRBindable.All)
            {
                var d1 = (pickup.transform.position - from).sqrMagnitude;
                if (d1 > range * range) continue;
                if (res == null)
                {
                    res = pickup;
                    continue;
                }

                var d2 = (res.transform.position - from).sqrMagnitude;
                if (d1 < d2)
                {
                    res = pickup;
                }
            }

            return res;
        }

        /// <summary>
        /// Calls action on any children of this object with a <see cref="IVRBindableListener"/> on it.
        /// This can be used for functional pickups, like a gun.
        /// </summary>
        /// <param name="hand">The hand providing the input</param>
        /// <param name="action">The action that was called</param>
        public void InputCallback(VRHand hand, IVRBindable.InputType inputType, HandInput.InputWrapper action)
        {
            var listeners = GetComponentsInChildren<IVRBindableListener>();
            foreach (var listener in listeners)
            {
                listener.InputCallback(hand, this, inputType, action);
            }
        }
    }
}