using HandyVR.Bindables;
using UnityEngine;

namespace HandyVR.Switches
{
    [AddComponentMenu("HandyVR/Float Drivers/Slider", Reference.AddComponentMenuOrder.Submenu)]
    public sealed class VRSlider : FloatDriver
    {
        [Tooltip("The distance the handle can travel along the Y axis from the center.")]
        [SerializeField] private float extents;
        
        private VRHandle handle;
        private new Rigidbody rigidbody;

        // Remap the local position of the handle so its offset along the Y axis represents a value between 0 and 1.
        public override float Value => handle.transform.localPosition.z / extents * 0.5f + 0.5f;

        private void Awake()
        {
            // Autocomplete object if partially setup.
            rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
            
            handle = GetComponentInChildren<VRHandle>();
            SetupHandle();
        }

        private void SetupHandle()
        {
            var rigidbody = handle.gameObject.GetOrAddComponent<Rigidbody>();
            rigidbody.mass = 0.02f;
            rigidbody.drag = 6.0f;
            rigidbody.useGravity = false;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            
            var joint = handle.gameObject.GetOrAddComponent<ConfigurableJoint>();
            joint.connectedBody = this.rigidbody;
            
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Limited;
            
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = Vector3.zero;
            
            joint.linearLimit = new SoftJointLimit()
            {
                limit = extents,
                bounciness = 0.0f,
                contactDistance = 0.0f,
            };
        }
    }
}