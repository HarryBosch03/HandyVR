using System;
using System.Collections.Generic;
using HandyVR.Bindables;
using HandyVR.Interfaces;
using HandyVR.Player.Input;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace HandyVR.Player
{
    /// <summary>
    /// Main controller for the Players Hands.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("HandyVR/VR Hand", Reference.AddComponentMenuOrder.Components)]
    public sealed class VRHand : MonoBehaviour, IHasValidationChecks
    {
        [Space]
        public Chirality chirality;
        [Tooltip("Chirality of the hand model used.")]
        public Chirality defaultHandModelChirality;
        [Tooltip("Axis to flip the hand model on if there is a chiral mismatch")]
        public Vector3 flipAxis = Vector3.right;
        [Tooltip("The Transform used to point at things, the Z axis should be the direction of the index finger")]
        public Transform pointTransform;
        [Tooltip("The root object for the Visible Model and Colliders for the hand")]
        public Transform handModel;
        [Tooltip("The Position the hands will be transposed to when reset")]
        public Transform resetTransform;
        public float resetDistanceThreshold = 2.0f;

        private HandInput input;
        private IVRHandMovement movement;
        private IVRHandBinding binding;
        private IVRHandModule[] modules;

        public HandInput Input => input;
        public IVRHandBinding BindingController => binding;
        public IVRHandMovement Movement => movement;
        public Transform PointTransform => pointTransform ? pointTransform : transform;
        public VRBinding ActiveBinding => binding.ActiveBinding;
        public Rigidbody Rigidbody { get; private set; }
        public Transform Target { get; private set; }
        public Collider[] Colliders { get; private set; }
        public bool Flipped => chirality != defaultHandModelChirality;

        public List<ValidationCheck> ValidationList => new()
        {
            IHasValidationChecks.HasComponent(typeof(IVRHandMovement)),
            IHasValidationChecks.HasComponent(typeof(IVRHandBinding)),
        };

        private void Awake()
        {
            if (!resetTransform) resetTransform = transform.root;

            // Clear Parent to stop the transform hierarchy from fucking up physics.
            // Group objects to keep hierarchy neat.
            Target = new GameObject($"{name} [Hand Target]").transform;
            Target.SetParent(transform.parent);
            Target.position = transform.position;
            Target.rotation = transform.rotation;
            Utility.Scene.BreakHierarchyAndGroup(transform);

            var chiralData = GetChiralData();
            gameObject.name = string.Format(chiralData.nameTemplate, gameObject.name);
            Target.gameObject.name = string.Format(chiralData.nameTemplate, Target.gameObject.name);

            // Create input module with correct controller.
            input = new HandInput(chiralData.controller);
            
            Rigidbody = gameObject.GetOrAddComponent<Rigidbody>();
            Colliders = GetComponentsInChildren<Collider>();

            // Initialize Submodules.

            movement = GetComponent<IVRHandMovement>();
            binding = GetComponent<IVRHandBinding>();

            modules = GetComponents<IVRHandModule>();
            foreach (var module in modules)
            {
                module.Init(this);
            }

            // Flip hand if chiral mismatch.
            if (Flipped && handModel)
            {
                var scale = Vector3.Reflect(Vector3.one, flipAxis.normalized);
                handModel.localScale = scale;
            }
        }

        private void FixedUpdate()
        {
            if (!Input.Active) return;

            // Update Submodules.
            movement.MoveTo(Target.position, Target.rotation);

            if ((transform.position - Target.position).sqrMagnitude > resetDistanceThreshold * resetDistanceThreshold)
            {
                ResetHands();
            }
        }

        private void Update()
        {
            // Update Submodules.
            Input.Update();

            if (!Input.Active)
            {
                SetModelVisibility(false);
                return;
            }
            
            // Update Targets Pose.
            Target.localPosition = Input.Position;
            Target.localRotation = Input.Rotation;
            
            if (ActiveBinding)
            {
                // Hide hand model if we are bound to something
                SetModelVisibility(false);
                
                // Pass inputs to bound object.
                ActiveBinding.bindable.InputCallback(this, IVRBindable.InputType.Trigger, Input.Trigger);
            }
            else
            {
                // Show hand and animate if unbound.
                SetModelVisibility(true);
            }
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // Callback for movement collision.
            movement.OnCollision(collision);
        }

        public enum Chirality
        {
            Left,
            Right,
        }

        public ChiralData GetChiralData()
        {
            return chirality switch
            {
                Chirality.Left => new ChiralData
                {
                    controller = () => XRController.leftHand, 
                    nameTemplate = "{0}.L",
                },
                Chirality.Right => new ChiralData
                {
                    controller = () => XRController.rightHand, 
                    nameTemplate = "{0}.R",
                },
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void SetModelVisibility(bool state)
        {
            if (!handModel) return;
            handModel.gameObject.SetActive(state);
        }
        
        public class ChiralData
        {
            public Func<XRController> controller;
            public string nameTemplate;
        }

        public void ResetHands()
        {
            foreach (var module in modules)
            {
                module.OnHandReset();
            }
        }
    }
}