using HandyVR.Interfaces;
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

        [Tooltip("How much of the calculated force is applied per frame. Lower numbers result in a smoother result but can cause rubber-banding.")] [SerializeField] [Range(0.0f, 1.0f)]
        private float forceScaling = 1.0f;

        [Space] 
        [SerializeField] private float dislocationSpring = 100.0f;
        [SerializeField] private float dislocationDamper = 8.0f;
        [SerializeField] private float dislocationDeadzone = 0.1f;
        [SerializeField] private float dislocationSpeed = 15.0f;
        [SerializeField] private LineRenderer dislocationLines;

        private VRHand hand;

        private Rigidbody rb;
        private bool dislocated;
        private float dislocationPercent;

        private float baseLineWidth;
        private int lineSubdivisions;

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

            if (!dislocationLines) dislocationLines = GetComponentInChildren<LineRenderer>();

            if (dislocationLines)
            {
                baseLineWidth = dislocationLines.widthMultiplier;
                lineSubdivisions = Mathf.Max(0, dislocationLines.positionCount - 2);
            }
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

            Vector3 force;
            Vector3 torque;

            if (dislocated)
            {
                force = (newPosition - rb.position) * dislocationSpring - rb.velocity * dislocationDamper;

                var delta = newRotation * Quaternion.Inverse(rb.rotation);
                delta.ToAngleAxis(out var angle, out var axis);
                torque = axis * (angle * Mathf.Deg2Rad * dislocationSpring) - rb.angularVelocity * dislocationDamper;
            }
            else
            {
                force = ((newPosition - rb.position) / Time.deltaTime - rb.velocity) / Time.deltaTime;

                var delta = newRotation * Quaternion.Inverse(rb.rotation);
                delta.ToAngleAxis(out var angle, out var axis);
                torque = (axis * (angle * Mathf.Deg2Rad / Time.deltaTime) - rb.angularVelocity) / Time.deltaTime;
            }

            // Add a force that effectively cancels out the current velocity, and translates the hand to the target position.
            // Using a force instead is purely for collision and stability, MovePosition ended up causing horrific desync.
            rb.AddForce(force * forceScaling, ForceMode.Acceleration);

            // Do the same with a torque, match the current target rotation.
            rb.AddTorque(torque * forceScaling, ForceMode.Acceleration);

            CheckIfDislocated(newPosition);
        }

        private void CheckIfDislocated(Vector3 newPosition)
        {
            var dislocation = (newPosition - rb.position + rb.velocity * Time.deltaTime).magnitude - dislocationDeadzone;
            var s = dislocation > 0.0f;
            dislocationPercent += dislocation * dislocationSpeed * Time.deltaTime;

            dislocated = dislocationPercent > 1.0f;
            dislocationPercent = Mathf.Clamp01(dislocationPercent);

            dislocationLines.enabled = dislocated;
            dislocationLines.SetLine(rb.position, newPosition, true, lineSubdivisions);
            dislocationLines.widthMultiplier = baseLineWidth * dislocationPercent;
        }

        public void OnCollision(Collision collision)
        {
            hand.Input.Rumble(rumbleMagnitude, 0.0f);
        }
    }
}