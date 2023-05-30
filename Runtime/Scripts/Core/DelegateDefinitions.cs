using UnityEngine;

namespace HandyVR.Core
{
    public delegate Vector3 PAnchor();
    public delegate Quaternion RAnchor();

    public delegate void Anchor(out Vector3 position, out Quaternion rotation);
}