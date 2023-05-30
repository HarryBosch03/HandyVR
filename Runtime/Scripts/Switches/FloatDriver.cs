using UnityEngine;

namespace HandyVR.Switches
{
    /// <summary>
    /// Base used for components that can drive an object with a float value, like a <see cref="VRSlider"/>
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public abstract class FloatDriver : MonoBehaviour
    {
        public virtual float Value { get; protected set; }
    }
}
