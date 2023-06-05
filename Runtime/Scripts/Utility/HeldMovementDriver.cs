using UnityEngine;

namespace HandyVR.Utility
{
    [System.Serializable]
    public class HeldMovementDriver
    {
        [SerializeField] private float spring = 100.0f;
        [SerializeField] private float damper = 10.0f;
        [SerializeField] private float rotationScale = 0.005f;
        [SerializeField] private float blendOffset = 0.05f;
        [SerializeField] private float blendRange = 15.0f;

        [SerializeField] [Range(0.0f, 1.0f)] private float translationBlend;
        [SerializeField] [Range(0.0f, 1.0f)] private float rotationBlend;
        
        public void Update(Rigidbody rb, Vector3 newPosition, Quaternion newRotation)
        {
            var idealForce = ((newPosition - rb.position) / Time.deltaTime - rb.velocity) / Time.deltaTime;
            
            var delta = newRotation * Quaternion.Inverse(rb.rotation);
            delta.ToAngleAxis(out var angleDeg, out var axis);
            var angle = angleDeg * Mathf.Deg2Rad;
            var angle2 = angle - Mathf.PI * 2.0f;
            if (Mathf.Abs(angle2) < Mathf.Abs(angle)) angle = angle2;
            
            var idealTorque = (axis * (angle / Time.deltaTime) - rb.angularVelocity) / Time.deltaTime;

            var springForce = (newPosition - rb.position) * spring - rb.velocity * damper;
            var springTorque = axis * (angle * spring) - rb.angularVelocity * damper;

            translationBlend = Mathf.Clamp01(((newPosition - rb.position).magnitude - blendOffset) * blendRange);
            var force = Vector3.Lerp(idealForce, springForce, translationBlend);

            rotationBlend = Mathf.Clamp01((Mathf.Abs(angle) * rotationScale - blendOffset) * blendRange);
            var torque = Vector3.Lerp(idealTorque, springTorque, rotationBlend);
            
            // Add a force that effectively cancels out the current velocity, and translates the hand to the target position.
            // Using a force instead is purely for collision and stability, MovePosition ended up causing horrific desync.
            rb.AddForce(force, ForceMode.Acceleration);
            
            // Do the same with a torque, match the current target rotation.
            rb.AddTorque(torque, ForceMode.Acceleration);
        }
    }
}