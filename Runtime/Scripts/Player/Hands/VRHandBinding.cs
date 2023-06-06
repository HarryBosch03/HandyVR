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

        [Space] 
        [SerializeField] private Vector3 bindingOffsetTranslation;
        [SerializeField] private Vector3 bindingOffsetRotation;

        [Space] 
        public LineRenderer lines;

        public VRHand Hand { get; private set; }

        public VRBinding ActiveBinding { get; private set; }
        public IVRBindable PointingAt { get; private set; }

        public Vector3 BindingPosition => Hand.Target.position + bindingOffsetTranslation;
        public Quaternion BindingRotation => Hand.Target.rotation * Quaternion.Euler(bindingOffsetRotation);

        public bool IsBindingFlipped => Hand.Flipped;

        public int BindingPriority => IVRBindingTarget.HandPriority;

        public void Init(VRHand hand)
        {
            this.Hand = hand;

            if (!lines) lines = GetComponentInChildren<LineRenderer>();
            if (lines) lines.enabled = false;
        }

        public void OnHandReset() => DeactivateBinding();

        public void LateUpdate()
        {
            PointingAt = null;
            if (lines) lines.enabled = false;

            // Call OnGrip if the Grip Input changed this frame.
            Hand.Input.Grip.ChangedThisFrame(OnGrip);

            // Bail if binding is valid.
            if (Hand.ActiveBinding) return;

            PointingAt = GetPointingAt();
            if (PointingAt != null && lines)
            {
                lines.SetLine(Hand.PointTransform.position, PointingAt.transform.position);
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

                var d1 = (bindable.transform.position - Hand.PointTransform.position).normalized;
                var d2 = Hand.PointTransform.forward;

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
            var ray = new Ray(Hand.PointTransform.position, other.transform.position - Hand.PointTransform.position);
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
            Hand.SetModelVisibility(true);
            
            Hand.Rigidbody.isKinematic = false;
            Hand.Rigidbody.velocity = Vector3.zero;
            Hand.Rigidbody.angularVelocity = Vector3.zero;
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