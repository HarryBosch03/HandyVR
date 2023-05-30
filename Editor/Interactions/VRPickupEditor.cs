using HandyVR.Bindables.Pickups;
using UnityEditor;
using static UnityEditor.EditorGUILayout;
using static UnityEngine.GUILayout;

namespace HandyVR.Editor.Interactions
{
    [CustomEditor(typeof(VRPickup))]
    public class VRPickupEditor : Editor<VRPickup>
    {
        private bool handlesControlBoundOffset;
        private bool handlesControlFlipped;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            Section("Socket Settings", () => { PropertyField(serializedObject.FindProperty("bindingType")); });

            PropertyField(serializedObject.FindProperty("boundPoses"));
            Section("Bound Pose Settings", () =>
            {
                PropertyField(serializedObject.FindProperty("defaultPose"));
                PropertyField(serializedObject.FindProperty("socketPose"));
                PropertyField(serializedObject.FindProperty("handCanUseSocketPose"));

                Space();

                var defaultPoseName = Target.DefaultPose.name;
                var socketPoseName = Target.SocketPose.name;

                using (new EditorGUI.DisabledScope(true))
                {
                    TextField("Default Pose", defaultPoseName);
                    TextField("Socket Pose", socketPoseName);
                }
            });

            Section("Editor Actions", () =>
            {
                if (Button("Edit Bound Offset"))
                {
                    handlesControlBoundOffset = !handlesControlBoundOffset;
                }

                if (Button("Edit Flipped Offset"))
                {
                    handlesControlFlipped = !handlesControlFlipped;
                }
            });

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
        }
    }
}