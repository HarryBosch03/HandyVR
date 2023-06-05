using HandyVR.Interfaces;
using HandyVR.Utility;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    /// <summary>
    /// Submodule for PlayerHand that manages the movement and collision.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public class VRHandMovement : MonoBehaviour, IVRHandMovement
    {
        [Tooltip("Magnitude of the rumble used when the hand is dragged along a surface")] [SerializeField] [Range(0.0f, 1.0f)]
        private float rumbleMagnitude = 0.2f;

        [Space] 
        [SerializeField] private HeldMovementDriver movementDriver;

        private VRHand hand;

        private Rigidbody rb;

        private float baseLineWidth;
        private int lineSubdivisions;

        // ReSharper disable once InconsistentNaming
        private PhysicMaterial handMaterial_DoNotUse;
        private PhysicMaterial HandMaterial
        {
            get
            {
                if (handMaterial_DoNotUse) return handMaterial_DoNotUse;
                
                var mat = new PhysicMaterial();
                handMaterial_DoNotUse = mat;

                mat.name = "[AUTO-GENERATED] Hand Material";
                mat.bounciness = 0.0f;
                mat.dynamicFriction = 0.0f;
                mat.staticFriction = 0.0f;
                mat.bounceCombine = PhysicMaterialCombine.Multiply;
                mat.frictionCombine = PhysicMaterialCombine.Multiply;
                mat.bounciness = 0.0f;
                return handMaterial_DoNotUse;
            }
        }

        public void Init(VRHand hand)
        {
            this.hand = hand;
            foreach (var collider in hand.Colliders)
            {
                collider.isTrigger = false;
            }

            rb = hand.Rigidbody;
            rb.useGravity = false;
            rb.angularDrag = 0.0f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.mass = 0.01f;

            foreach (var collider in hand.Colliders)
            {
                collider.material = HandMaterial;
            }
        }

        public void OnHandReset()
        {
            transform.position = hand.ResetTransform.position;
            transform.rotation = hand.ResetTransform.rotation;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        public void MoveTo(Vector3 newPosition, Quaternion newRotation)
        {
            // If the hand has a binding, just teleport hand to tracked position, the
            // bound object will have their own collision.
            if (hand.ActiveBinding)
            {
                rb.isKinematic = true;

                transform.position = newPosition;
                transform.rotation = newRotation;

                return;
            }

            rb.isKinematic = false;
            rb.centerOfMass = Vector3.zero;
            
            movementDriver.Update(rb, newPosition, newRotation);
        }

        public void OnCollision(Collision collision)
        {
            hand.Input.Rumble(rumbleMagnitude, 0.0f);
        }
    }
}