using System.Drawing.Printing;
using UnityEditor;
using UnityEngine;
using UnityEditor.XR.LegacyInputHelpers;

namespace HandyVR.Editor
{
    public static class EditorActions
    {
        [MenuItem("GameObject/HandyVR/Create Handy VR Rig")]
        public static void CreateHandyVRRig()
        {
            var cameraOffset = Object.FindObjectOfType<CameraOffset>();
            if (!cameraOffset)
            {
                EditorApplication.ExecuteMenuItem("GameObject/XR/Convert Main Camera To XR Rig");
                cameraOffset = Object.FindObjectOfType<CameraOffset>();
            }

            CreateHandyVRRig(cameraOffset);
        }

        public static void CreateHandyVRRig(CameraOffset root)
        {
            var camera = root.GetComponentInChildren<Camera>();
            var cameraOffset = camera.transform.parent;

            var hands = new GameObject[2];
            
            var lh = hands[0] = new GameObject("HandyVR Hand.L");
            var rh = hands[1] = new GameObject("HandyVR Hand.R");

            foreach (var hand in hands)
            {
                hand.transform.SetParent(camera.transform);
                
            }
        }
    }
}