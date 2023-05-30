using HandyVR.Player;

namespace HandyVR.Interfaces
{
    public interface IVRHandModule : IBehaviour
    {
        void Init(VRHand hand);
    }
}