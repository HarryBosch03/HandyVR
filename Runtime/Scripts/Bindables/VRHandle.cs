using UnityEngine;

namespace HandyVR.Bindables
{
    /// <summary>
    /// A handle that can be added to a child of a physics object with constraints,
    /// allowing for complex behaviours like hinges or sliders.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("HandyVR/Handle", Reference.AddComponentMenuOrder.Components)]
    public sealed class VRHandle : VRBindable
    {
        private bool wasBound;
        private Vector3 translationOffset;
        private Quaternion rotationOffset;

        private new Rigidbody rigidbody;
        public override Rigidbody Rigidbody => rigidbody;

        public override void OnBindingActivated(VRBinding binding)
        {
            base.OnBindingActivated(binding);

            translationOffset = Quaternion.Inverse(BindingRotation) * (rigidbody.position - BindingPosition);
            rotationOffset = Quaternion.Inverse(BindingRotation) * rigidbody.rotation;
        }
        
        private void Start()
        {
            rigidbody = GetComponentInParent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!ActiveBinding) return;

            // Match the hands position through a simple spring damper, applied to the handles parent at the handles position.
            var target = BindingPosition + BindingRotation * translationOffset;
            var diff = (target - rigidbody.position);
            var pointVelocity = rigidbody.velocity;
            var force = (diff / Time.deltaTime - pointVelocity) / Time.deltaTime;

            rigidbody.AddForce(force, ForceMode.Acceleration);

            var delta = BindingRotation * rotationOffset * Quaternion.Inverse(rigidbody.rotation);
            delta.ToAngleAxis(out var angle, out var axis);

            // Calculate a torque to move the rigidbody to the target rotation with zero angular velocity.
            var torque = (axis * (angle * Mathf.Deg2Rad / Time.deltaTime) - rigidbody.angularVelocity) / Time.deltaTime;
            rigidbody.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}