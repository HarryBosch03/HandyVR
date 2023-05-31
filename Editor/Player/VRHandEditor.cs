using HandyVR.Player;
using UnityEditor;

namespace HandyVR.Editor.Player
{
    [CustomEditor(typeof(VRHand))]
    public class VRHandEditor : Editor<VRHand>
    {
        private VRHand hand;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            IHasValidationChecks.DrawGUI(Target);
        }
    }
}