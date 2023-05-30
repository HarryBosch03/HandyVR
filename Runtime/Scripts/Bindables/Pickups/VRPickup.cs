using System;
using System.Collections.Generic;
using HandyVR.Bindables.Targets;
using UnityEngine;

namespace HandyVR.Bindables.Pickups
{
    /// <summary>
    /// Physics object that can be picked up by the player.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("HandyVR/Basic Pickup", Reference.AddComponentMenuOrder.Components)]
    public sealed class VRPickup : VRBindable
    {
        [Tooltip("A optional binding type used for sockets.")] [SerializeField]
        private VRBindingType bindingType;

        [Space] [Tooltip("The magnitude of the force applied to a Detached Binding")] [SerializeField]
        private float detachedBindingSpeed = 400.0f;

        [Tooltip("The Minimum force applied to the Detached Binding")] [SerializeField]
        private float detachedBindingInvalidationTime = 1.0f;

        [SerializeField] [Range(0.0f, 1.0f)] private float detachedBindingInvalidationThreshold = 0.5f;

        [Tooltip("List of offset for when the object is Bound.")] [SerializeField]
        private List<BoundPose> boundPoses;

        [SerializeField] private int defaultPose;
        [SerializeField] private int socketPose;
        [SerializeField] private bool handCanUseSocketPose;

        private readonly List<ColliderData> colliderData = new();

        private int boundPoseIndex;
        private bool detachedBinding;
        private Vector3 lastPosition;
        private float detachedBindingInvalidationTimer;

        private new Rigidbody rigidbody;
        public override Rigidbody Rigidbody => rigidbody;
        public BoundPose DefaultPose => GetPose(HandPoseIndex(defaultPose));
        public BoundPose SocketPose => GetPose(socketPose);
        public BoundPose CurrentBoundPose => GetPose(boundPoseIndex);

        // Physics material for when the object is held, this is to stop wierd jitter caused from bouncy objects.
        // ReSharper disable once InconsistentNaming
        private static PhysicMaterial overridePhysicMaterial_DoNotUse;

        private static PhysicMaterial OverridePhysicMaterial
        {
            get
            {
                // Lazy Initialization.
                if (overridePhysicMaterial_DoNotUse) return overridePhysicMaterial_DoNotUse;

                overridePhysicMaterial_DoNotUse = new PhysicMaterial();
                overridePhysicMaterial_DoNotUse.name =
                    "VR Pickup | Override Physics Material [SHOULD ONLY BE ON WHILE OBJECT IS HELD]";
                overridePhysicMaterial_DoNotUse.bounciness = 0.0f;
                overridePhysicMaterial_DoNotUse.dynamicFriction = 0.0f;
                overridePhysicMaterial_DoNotUse.staticFriction = 0.0f;
                overridePhysicMaterial_DoNotUse.bounceCombine = PhysicMaterialCombine.Multiply;
                overridePhysicMaterial_DoNotUse.frictionCombine = PhysicMaterialCombine.Multiply;
                return overridePhysicMaterial_DoNotUse;
            }
        }

        public VRBindingType BindingType => bindingType;

        public static readonly BoundPose DefaultBoundPose = new()
        {
            name = "Class Default",
        };

        #region Unity Messages

        private void Awake()
        {
            rigidbody = gameObject.GetOrAddComponent<Rigidbody>();

            // Force rigidbody to use continuous collision for stability when held.
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                colliderData.Add(new ColliderData(collider));
            }
        }

        private void FixedUpdate()
        {
            MoveIfBound();
        }

        #endregion

        #region Overrides

        public override void OnBindingActivated(VRBinding binding)
        {
            base.OnBindingActivated(binding);

            detachedBinding = true;
            detachedBindingInvalidationTimer = 0.0f;

            // Cache each colliders material and override it with the special held material.
            foreach (var data in colliderData)
            {
                data.lastMaterial = data.collider.material;
                data.collider.sharedMaterial = OverridePhysicMaterial;
            }

            ChangePose(ActiveBinding, i => defaultPose);
        }

        public override void OnBindingDeactivated(VRBinding binding)
        {
            base.OnBindingDeactivated(binding);

            // Restore original physics material to each collider from cache.
            foreach (var data in colliderData)
            {
                data.collider.sharedMaterial = data.lastMaterial;
            }
        }

        #endregion

        #region Pose

        public int HandPoseIndex(int i) => handCanUseSocketPose || i != boundPoseIndex ? i : i + 1;

        public void CycleHoldPose()
        {
            ChangePose(ActiveBinding, i => i + 1);
        }

        public void ChangePose(VRBinding binding, Func<int, int> change)
        {
            if (binding.target is VRSocket)
            {
                boundPoseIndex = socketPose;
                return;
            }

            boundPoseIndex = HandPoseIndex(change(boundPoseIndex));
        }

        public BoundPose GetPose(int i)
        {
            if (boundPoses == null || boundPoses.Count <= 0) return DefaultBoundPose;
            return boundPoses.Ring(i);
        }

        #endregion

        private void MoveIfBound()
        {
            if (!ActiveBinding) return;

            if (UpdateDetachedBinding()) return;

            var translationOffset = CurrentBoundPose.translationOffset;
            var rotationOffset = CurrentBoundPose.rotationOffset;
            var additionalFlipRotation = CurrentBoundPose.additionalFlipRotation;

            // Calculate force needed to be applied to translate the object from its current position
            // to the binding position ( + offset ) with zero velocity.
            var force = (BindingPosition + translationOffset - rigidbody.position) / Time.deltaTime -
                        rigidbody.velocity;
            rigidbody.AddForce(force, ForceMode.VelocityChange);

            // Calculate the finalized rotational offset.
            var offset = Quaternion.Euler(rotationOffset);
            if (BindingFlipped) offset *= Quaternion.Euler(additionalFlipRotation);

            // Calculate the angle axis rotation needed to move from our current rotation to the target ( + offset )
            var delta = BindingRotation * offset * Quaternion.Inverse(rigidbody.rotation);
            delta.ToAngleAxis(out var angle, out var axis);

            // Calculate a torque to move the rigidbody to the target rotation with zero angular velocity.
            var torque = axis * (angle * Mathf.Deg2Rad / Time.deltaTime) - rigidbody.angularVelocity;
            rigidbody.AddTorque(torque, ForceMode.VelocityChange);
        }

        private bool UpdateDetachedBinding()
        {
            if (!detachedBinding) return false;

            var rb = Rigidbody;
            var direction = ActiveBinding.target.BindingPosition - rb.position;
            var distance = direction.magnitude;
            direction /= distance;

            // If the object has the speed to get to the players hand this frame, remove the detached binding
            // and create an actual active binding, then bail.
            var frameSpeed = detachedBindingSpeed * Time.deltaTime / rb.mass;
            if (distance < frameSpeed * Time.deltaTime)
            {
                detachedBinding = false;
                return false;
            }

            // Apply the force
            var force = direction * frameSpeed - rb.velocity;
            rb.AddForce(force, ForceMode.VelocityChange);

            var delta = rb.position - lastPosition;
            var distanceTraveled = delta.magnitude;
            var invalid = distanceTraveled < frameSpeed * detachedBindingInvalidationThreshold;

            detachedBindingInvalidationTimer += (invalid ? 1.0f : -1.0f) * Time.deltaTime / detachedBindingInvalidationTime;
            if (detachedBindingInvalidationTimer >= 1.0f)
            {
                detachedBinding = false;
                ActiveBinding.Deactivate();
                return true;
            }
            detachedBindingInvalidationTimer = Mathf.Clamp01(detachedBindingInvalidationTimer);

            lastPosition = rb.position;
            
            return true;
        }

        #region Internal Classes

        /// <summary>
        /// Class for caching collider data for restoration when a pickup is released.
        /// </summary>
        private class ColliderData
        {
            public readonly Collider collider;
            public PhysicMaterial lastMaterial;

            public ColliderData(Collider collider)
            {
                this.collider = collider;
                lastMaterial = collider.sharedMaterial;
            }
        }

        [Serializable]
        public class BoundPose
        {
            public string name = "New Bound Pose";
            public Vector3 translationOffset;
            public Vector3 rotationOffset;
            public Vector3 additionalFlipRotation;
        }

        #endregion
    }
}