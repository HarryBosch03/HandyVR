using HandyVR.Input.UI;
using HandyVR.Player;
using HandyVR.Player.Hands;
using UnityEditor;
using UnityEngine;
using UnityEditor.XR.LegacyInputHelpers;

namespace HandyVR.Editor
{
    public static class EditorActions
    {
        private const string UndoKey = "Create HandyVR Rig";

        [MenuItem("GameObject/HandyVR/Create HandyVR Rig")]
        public static void CreateHandyVRRig()
        {
            var cameraOffset = Object.FindObjectOfType<CameraOffset>();
            if (!cameraOffset)
            {
                Debug.Log("No XR Rig was found in scene, creating one automatically.");
                EditorApplication.ExecuteMenuItem("GameObject/XR/Convert Main Camera To XR Rig");
                cameraOffset = Object.FindObjectOfType<CameraOffset>();
            }

            CreateHandyVRRig(cameraOffset);
        }

        public static void CreateHandyVRRig(CameraOffset root)
        {
            if (root.GetComponentInChildren<VRHand>())
            {
                Debug.LogError("Cannot create HandyVR Rig, XR rig hierarchy already contains an object with a VRHand Component", root);
                Selection.objects = new Object[] { root.gameObject };
                EditorGUIUtility.PingObject(root.gameObject);
                return;
            }

            var camera = root.GetComponentInChildren<Camera>();
            var cameraOffset = camera.transform.parent;

            var hands = new GameObject[2];


            var lh = hands[0] = new GameObject("HandyVR Hand.L");
            var rh = hands[1] = new GameObject("HandyVR Hand.R");

            var defaultLineMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat");

            foreach (var gameObject in hands)
            {
                gameObject.transform.SetParent(cameraOffset.transform);
                var hand = gameObject.AddComponent<VRHand>();
                var hMovement = gameObject.AddComponent<VRHandMovement>();
                var hBinding = gameObject.AddComponent<VRHandBinding>();
                var xrPointer = gameObject.AddComponent<XRPointer>();

                var bindingLines = new GameObject("Binding Lines").AddComponent<LineRenderer>();
                bindingLines.transform.SetParent(gameObject.transform);
                bindingLines.widthMultiplier = 0.02f;
                bindingLines.numCapVertices = 8;
                bindingLines.materials = new[] { defaultLineMaterial };
                bindingLines.enabled = false;
                hBinding.lines = bindingLines;

                var uiCursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                uiCursor.name = "UI Pointer";
                uiCursor.transform.SetParent(gameObject.transform);
                uiCursor.transform.localScale = Vector3.one * 0.005f;
                xrPointer.cursor = uiCursor;

                var pointTransform = new GameObject("Point Transform").transform;
                pointTransform.SetParent(gameObject.transform);
                pointTransform.rotation = Quaternion.Euler(75.0f, 0.0f, 0.0f);
                hand.pointTransform = pointTransform;

                var handModel = new GameObject("Hand Model").transform;
                handModel.transform.SetParent(gameObject.transform);
                hand.handModel = handModel;

                hand.chirality = gameObject == lh ? VRHand.Chirality.Left : VRHand.Chirality.Right;
            }

            Selection.objects = new Object[] { lh, rh };
            EditorGUIUtility.PingObject(rh);
        }
    }
}