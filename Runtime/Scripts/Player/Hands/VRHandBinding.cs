using System.Collections.Generic;
using HandyVR.Bindables;
using HandyVR.Interfaces;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    /// <summary>
    /// Submodule used for managing the <see cref="VRHand"/>s current binding.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public class VRHandBinding : MonoBehaviour, IVRHandBinding
    {
        [Tooltip("The radius to search for a pickup when the grab button is pressed")] [SerializeField]
        private float pickupRange = 0.2f;

        [Tooltip("The angle of the cone used to find Pickups to create Bindings")] [SerializeField]
        private float pointingAtAngle = 15.0f;

        [Space] [SerializeField] private Vector3 bindingOffsetTranslation;
        [SerializeField] private Vector3 bindingOffsetRotation;

        [Space] [SerializeField] private LineRenderer lines;

        private VRHand hand;

        public VRBinding ActiveBinding { get; private set; }
        public IVRBindable PointingAt { get; private set; }

        public Vector3 BindingPosition => hand.Target.position + bindingOffsetTranslation;
        public Quaternion BindingRotation => hand.Target.rotation * Quaternion.Euler(bindingOffsetRotation);

        public bool IsBindingFlipped => hand.Flipped;

        public int BindingPriority => IVRBindingTarget.HandPriority;

        public void Init(VRHand hand)
        {
            this.hand = hand;

            if (!lines) lines = GetComponentInChildren<LineRenderer>();
            lines.enabled = false;
        }

        public void LateUpdate()
        {
            PointingAt = null;
            lines.enabled = false;

            // Call OnGrip if the Grip Input changed this frame.
            hand.Input.Grip.ChangedThisFrame(OnGrip);

            // Bail if binding is valid.
            if (hand.ActiveBinding) return;

            PointingAt = GetPointingAt();
            if (PointingAt != null)
            {
                lines.SetLine(hand.PointRef.position, PointingAt.transform.position);
            }
        }

        /// <summary>
        /// Creates binding between and and Pickup
        /// </summary>
        /// <param name="bindable">The Pickup to bind</param>
        private void Bind(IVRBindable bindable)
        {
            // Create binding.
            new VRBinding(bindable, this);
        }

        /// <summary>
        /// Returns a Bindable that is being pointed at the most, while also being in line of sight.
        /// </summary>
        /// <returns>The Object being pointed at the most. Will return null if nothing can be found.</returns>
        private IVRBindable GetPointingAt()
        {
            // Method used to find the bindable being pointed at the most.
            float getScore(IVRBindable bindable)
            {
                // Do not use object we cannot see.
                if (!CanSee(bindable)) return -1.0f;

                var d1 = (bindable.transform.position - hand.PointRef.position).normalized;
                var d2 = hand.PointRef.forward;

                // Reciprocate the result to find the object with the smallest dot product.
                return 1.0f / (Mathf.Acos(Vector3.Dot(d1, d2)) * Mathf.Rad2Deg);
            }

            return Utility.Collections.Best(IVRBindable.All, getScore, 1.0f / (pointingAtAngle * 2.0f));
        }

        /// <summary>
        /// Is there a clear line of sight from the Index Finger to the Bindable.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>Is there a clear line of sight from the Index Finger to the Bindable.</returns>
        private bool CanSee(IVRBindable other)
        {
            var ray = new Ray(hand.PointRef.position, other.transform.position - hand.PointRef.position);
            return Physics.Raycast(ray, out var hit) && other.transform.IsChildOf(hit.transform);
        }

        private void OnGrip(bool state)
        {
            DeactivateBinding();

            if (state) OnGripPressed();
        }

        private void DeactivateBinding()
        {
            // Ignore if no active binding.
            if (!ActiveBinding) return;

            // Deactivate the current binding.
            ActiveBinding.Deactivate();

            // Preemptively reset the state of the hand model and rigidbody.
            hand.HandModel.gameObject.SetActive(true);

            hand.Rigidbody.isKinematic = false;
            hand.Rigidbody.velocity = Vector3.zero;
            hand.Rigidbody.angularVelocity = Vector3.zero;
        }

        private void OnGripPressed()
        {
            // Try to pickup the closest object to the hand.
            // If none can be found, try to create a detached binding with
            // whatever is being pointed at.
            var pickup = VRBindable.GetBindable(transform.position, pickupRange);
            if (pickup == null) pickup = GetPointingAt();
            if (pickup == null) return;

            Bind(pickup);
        }

        public void OnBindingActivated(VRBinding binding)
        {
            ActiveBinding = binding;
        }

        public void OnBindingDeactivated(VRBinding binding)
        {
        }
    }
}