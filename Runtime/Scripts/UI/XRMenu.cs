using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace HandyVR.UI
{
    // TODO Finish
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class XRMenu : MonoBehaviour
    {
        private const float DepthOffset = 0.1f;
        
        [SerializeField] private bool pauseGame;
        [SerializeField] private InputAction openAction;
        [SerializeField] private Vector3 offset;

        // ReSharper disable once InconsistentNaming
        private bool isOpen_DoNotUse;

        private CanvasGroup group;
        private CanvasGroup canvasGroup;

        private static Vector3 menuPosition;
        private static Quaternion menuOrientation;

        public bool IsOpen
        {
            get => isOpen_DoNotUse;
            set
            {
                if (isOpen_DoNotUse == value) return;
                isOpen_DoNotUse = value;
                UpdateOpenState();
            }
        }

        public int Depth { get; private set; }
        public static event Action<XRMenu, bool> PauseStateChangedEvent;
        public static bool IsPaused { get; private set; }
        public static List<XRMenu> Stack { get; } = new();

        private void Awake()
        {
            Utility.Input.BindAndEnable(openAction, ToggleOpen);
            
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            IsOpen = false;
        }

        private void ToggleOpen() => IsOpen = !IsOpen;

        private void UpdateOpenState()
        {
            if (IsOpen)
            {
                if (Camera.main && Stack.Count == 0)
                {
                    menuPosition = Camera.main.transform.position;
                    menuOrientation = Quaternion.Euler(0.0f, Camera.main.transform.eulerAngles.y, 0.0f);
                }
                Stack.Add(this);
            }
            else
            {
                Stack.Remove(this);
                canvasGroup.alpha = 0.0f;
                canvasGroup.blocksRaycasts = false;
            }

            UpdateStackAll();
        }

        private void UpdateStackAll()
        {
            var lastPauseState = IsPaused;
            IsPaused = false;

            var depth = Stack.Count;
            foreach (var element in Stack)
            {
                element.Depth = --depth;
                element.UpdateStack();
                if (element.pauseGame)
                {
                    IsPaused = true;
                }
            }

            if (IsPaused != lastPauseState)
            {
                PauseStateChangedEvent?.Invoke(this, IsPaused);
            }
        }

        public void UpdateStack()
        {
            transform.position = menuPosition + menuOrientation * offset;
            transform.rotation = menuOrientation;

            transform.position += menuOrientation * new Vector3(-DepthOffset, -DepthOffset, DepthOffset);

            var top = Depth == 0;
            
            canvasGroup.alpha = top ? 1.0f : 0.5f;
            canvasGroup.blocksRaycasts = !top;
        }

        private void OnValidate()
        {
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas) return;
            
            canvas = new GameObject("Canvas").AddComponent<Canvas>();
            canvas.transform.SetParent(transform);

            canvas.worldCamera = Camera.main;

            canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.gameObject.AddComponent<TrackedDeviceRaycaster>();
        }
    }
}