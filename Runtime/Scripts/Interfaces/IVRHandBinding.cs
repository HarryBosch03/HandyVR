using HandyVR.Bindables;

namespace HandyVR.Interfaces
{
    public interface IVRHandBinding : IVRHandModule, IVRBindingTarget
    {
        VRBinding ActiveBinding { get; }
    }
}