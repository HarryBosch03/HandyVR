using UnityEngine;

namespace HandyVR.Interfaces
{
    public interface IVRHandMovement : IVRHandModule
    {
        void MoveTo(Vector3 newPosition, Quaternion newRotation);

        void OnCollision(Collision collision);
    }
}