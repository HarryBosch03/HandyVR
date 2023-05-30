using System.Collections.Generic;
using HandyVR.Player;
using HandyVR.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace HandyVR.Input.UI
{
    /// <summary>
    /// A Component to be added to the VR Hand which allows it to interact with canvases.
    /// </summary>
    [RequireComponent(typeof(VRHand))]
    [AddComponentMenu("HandyVR/Hands/XRPointer", Reference.AddComponentMenuOrder.Submenu)]
    public class XRPointer : MonoBehaviour
    {
        [Tooltip("An in world object that will appear on the UI where the hand is pointing")]
        [SerializeField] private GameObject cursor;
        [Tooltip("The additive scroll speed of the thumbstick.")]
        [SerializeField] private float scrollSpeed = 1000.0f;
        
        private ExtendedPointerEventData pointerData;
        private VRHand hand;

        public HandInput.InputWrapper TriggerAction => hand.Input.Trigger;
        public HandInput.InputWrapper ThumbstickXAction => hand.Input.ThumbstickX;
        public HandInput.InputWrapper ThumbstickYAction => hand.Input.ThumbstickY;
        public Transform PointRef => hand.PointRef;
        public static List<XRPointer> All { get; } = new ();

        private void Awake()
        {
            // Get pointer data linked to the event system.
            // ExtendedPointerEventData gives us control over tracked device position and rotation.
            pointerData = new ExtendedPointerEventData(EventSystem.current);
            hand = GetComponent<VRHand>();
        }

        private void OnEnable()
        {
            All.Add(this);
        }

        private void OnDisable()
        {
            All.Remove(this);
        }

        private void LateUpdate()
        {
            UpdateCursor();
        }

        /// <summary>
        /// Move cursor object to where the VR hand is pointing at, disable it if nothing is being pointed at.
        /// </summary>
        private void UpdateCursor()
        {
            if (!cursor) return;
            var hit = pointerData.pointerCurrentRaycast;
            if (!hit.isValid)
            {
                cursor.SetActive(false);
                return;
            }
            cursor.SetActive(true);
            cursor.transform.position = hit.worldPosition;
            cursor.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// Builds the data for the event system involving tracked devices, the rest is handled by the event system.
        /// </summary>
        /// <returns></returns>
        public ExtendedPointerEventData GetData()
        {
            pointerData.trackedDevicePosition = PointRef.position;
            pointerData.trackedDeviceOrientation = PointRef.rotation;
            pointerData.pointerType = UIPointerType.Tracked;
            pointerData.button = PointerEventData.InputButton.Left;
            pointerData.scrollDelta = new Vector2(-ThumbstickXAction.Value, ThumbstickYAction.Value) * scrollSpeed * Time.deltaTime;
            
            return pointerData;
        }
    }
}