using System;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace HandyVR.Player.Input
{
    /// <summary>
    /// Submodule used for managing the <see cref="VRHand"/> input.
    /// </summary>
    public sealed class HandInput
    {
        // Use function rather than just a reference in case the input device is yet to be connected or disconnects through play.
        public Func<XRController> Controller { get; }

        // Position and Rotation of the hands Target.
        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
        public bool Active { get; private set; }

        // Wrappers for controller input.
        public InputWrapper Grip { get; } = new();
        public InputWrapper Trigger { get; } = new();
        public InputWrapper Primary { get; } = new();
        public InputWrapper ThumbstickX { get; } = new() { pressPoint = 0.1f, };
        public InputWrapper ThumbstickY { get; } = new() { pressPoint = 0.1f, };

        public HandInput(Func<XRController> controller)
        {
            Controller = controller;
        }

        public void Update()
        {
            Active = false;
            
            var controller = Controller();
            if (controller == null) return;
            if ((InputTrackingState)controller.trackingState.ReadValue() == InputTrackingState.None) return;

            Active = true;
            
            Position = controller.devicePosition.ReadValue();
            Rotation = controller.deviceRotation.ReadValue();

            // Update inputs with appropriate controller type.
            switch (controller)
            {
                case OculusTouchController touchController:
                    Grip.Update(touchController.grip);
                    Trigger.Update(touchController.trigger);
                    ThumbstickX.Update(touchController.thumbstick, v => v.x);
                    ThumbstickY.Update(touchController.thumbstick, v => v.y);
                    Primary.Update(touchController.primaryButton);

                    Rotation *= Quaternion.Euler(-45.0f, 0.0f, 0.0f);

                    break;
                case UnityEngine.XR.OpenXR.Features.Interactions.OculusTouchControllerProfile.OculusTouchController
                    touchController:
                    Grip.Update(touchController.grip);
                    Trigger.Update(touchController.trigger);
                    ThumbstickX.Update(touchController.thumbstick, v => v.x);
                    ThumbstickY.Update(touchController.thumbstick, v => v.y);
                    Primary.Update(touchController.primaryButton);
                    break;
            }
        }

        /// <summary>
        /// Rumbles the controller if it exists and supports it.
        /// </summary>
        /// <param name="amplitude"></param>
        /// <param name="duration"></param>
        public void Rumble(float amplitude, float duration)
        {
            var controller = Controller();
            if (controller is XRControllerWithRumble rumble)
            {
                rumble.SendImpulse(amplitude, duration);
            }
        }

        /// <summary>
        /// Internal class for wrapping a bunch of input state to make other code cleaner.
        /// </summary>
        public class InputWrapper
        {
            public float pressPoint = 0.5f;

            private float lastValue;

            public float Value { get; private set; }
            public InputState State { get; private set; }
            public bool Down => Mathf.Abs(Value) > pressPoint;

            public void Update(InputControl<float> driver) => Update(driver.ReadValue());

            public void Update<T>(InputControl<T> driver, Func<T, float> getFloat) where T : struct =>
                Update(getFloat(driver.ReadValue()));

            public void Update(float value)
            {
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPaused) value = Value;
#endif

                Value = value;

                var state = Value > pressPoint;
                var lastState = lastValue > pressPoint;

                if (state)
                {
                    State = lastState ? InputState.Pressed : InputState.PressedThisFrame;
                }
                else
                {
                    State = lastState ? InputState.ReleasedThisFrame : InputState.Released;
                }

                lastValue = Value;
            }

            public enum InputState
            {
                Released,
                ReleasedThisFrame,
                Pressed,
                PressedThisFrame,
            }

            public void ChangedThisFrame(Action<bool> callback)
            {
                switch (State)
                {
                    case InputState.PressedThisFrame:
                        callback(true);
                        break;
                    case InputState.ReleasedThisFrame:
                        callback(false);
                        break;
                    case InputState.Pressed:
                    case InputState.Released:
                    default: break;
                }
            }
        }
    }
}