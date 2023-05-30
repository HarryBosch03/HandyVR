using System;
using HandyVR.Interfaces;
using UnityEngine;

namespace HandyVR.Bindables
{
    /// <summary>
    /// Class used to track a binding between a <see cref="bindable">Bindable Object</see> and a Pose.
    /// </summary>
    [Serializable]
    public class VRBinding
    {
        public readonly IVRBindable bindable;
        public readonly IVRBindingTarget target;
        public bool active;

        private Vector3 lastPosition;
        private Vector3 lastValue;

        /// <summary>
        /// Creates binding between a bindable object and a Pose Driver.
        /// </summary>
        /// <param name="bindable">Object that will be bound</param>
        /// <param name="target">Target Object to bind to</param>
        public VRBinding(IVRBindable bindable, IVRBindingTarget target)
        {
            if (bindable.ActiveBinding) bindable.ActiveBinding.Deactivate();

            this.bindable = bindable;
            this.target = target;
            active = true;

            bindable.OnBindingActivated(this);
            target.OnBindingActivated(this);
            
            Utility.Physics.IgnoreCollision(bindable.gameObject, target.gameObject, true);
        }

        /// <summary>
        /// Deactivates the binding, freeing the object bound.
        /// A binding cannot be reactivated, to rebind create another binding.
        /// </summary>
        public void Deactivate()
        {
            if (!active) return;
            
            active = false;
            bindable.OnBindingDeactivated(this);
            target.OnBindingDeactivated(this);
            
            Utility.DeferredCall.Wait(
                () => Utility.Physics.IgnoreCollision(bindable.gameObject, target.gameObject, false),
                new WaitForSeconds(0.2f));
        }

        public static Func<bool> GetAreClear(GameObject a, GameObject b)
        {
            var ac = a.GetComponentsInChildren<Collider>();
            var bc = b.GetComponentsInChildren<Collider>();
            
            return () =>
            {
                foreach (var collider in ac)
                foreach (var other in bc)
                {
                    if (!collider || !other) continue;
                    
                    if (Physics.ComputePenetration(collider, collider.transform.position, collider.transform.rotation, 
                            other, other.transform.position, other.transform.rotation, 
                            out _, out _)) return false;
                }

                return true;
            };
        }

        /// <summary>
        /// Returns if the binding is active and valid.
        /// </summary>
        /// <returns></returns>
        public bool Valid()
        {
            if (!active) return false;
            if (!IVRBindable.Valid(bindable)) return false;
            if (target == null) return false;
            
            return true;
        }

        /// <summary>
        /// Cast used to check if the binding is still doing stuff. Is null safe.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public static implicit operator bool(VRBinding binding)
        {
            return binding != null && binding.Valid();
        }
    }
}