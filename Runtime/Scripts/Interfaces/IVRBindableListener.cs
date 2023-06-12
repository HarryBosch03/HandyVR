using HandyVR.Bindables;
using HandyVR.Player;
using HandyVR.Player.Input;

namespace HandyVR.Interfaces
{
    /// <summary>
    /// Interface for when User wants to add custom logic to a Pickup, like a gun's trigger pull.
    /// </summary>
    public interface IVRBindableListener
    {
        void InputCallback(VRHand hand, VRBindable bindable, IVRBindable.InputType type, HandInput.InputWrapper input);

        void OnBindingActivated(VRBinding binding);
        void OnBindingDeactivated(VRBinding binding);
    }
}