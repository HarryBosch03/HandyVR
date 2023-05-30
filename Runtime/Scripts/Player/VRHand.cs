using System;
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
    public sealed class VRHand : MonoBehaviour
    {
        [Space]
        [SerializeField] private Chirality chirality;
        [Tooltip("Chirality of the hand model used.")]
        [SerializeField] private Chirality defaultHandModelChirality;
        [Tooltip("Axis to flip the hand model on if there is a chiral mismatch")]
        [SerializeField] private Vector3 flipAxis = Vector3.right;

        private HandInput input;
        private IVRHandMovement movement;
        private IVRHandBinding binding;
        
        private Transform pointRef;

        public HandInput Input => input;
        public IVRHandBinding BindingController => binding;
        public IVRHandMovement Movement => movement;
        public Transform HandModel { get; private set; }

        public VRBinding ActiveBinding => binding.ActiveBinding;
        public Rigidbody Rigidbody { get; private set; }
        public Transform Target { get; private set; }
        public Transform PointRef => pointRef ? pointRef : transform;
        public Collider[] Colliders { get; private set; }
        public bool Flipped => chirality != defaultHandModelChirality;
        
        private void Awake()
        {
            // Clear Parent to stop the transform hierarchy from fucking up physics.
            // Group objects to keep hierarchy neat.
            Target = transform.parent;
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

            foreach (var module in GetComponents<IVRHandModule>())
            {
                module.Init(this);
            }
            
            // Cache Hierarchy.
            pointRef = transform.DeepFind("Point Ref");
            HandModel = transform.DeepFind("Model");
            
            // Flip hand if chiral mismatch.
            if (Flipped)
            {
                var scale = Vector3.Reflect(Vector3.one, flipAxis.normalized);
                HandModel.localScale = scale;
            }
        }

        private void FixedUpdate()
        {
            if (!Input.Active) return;

            // Update Submodules.
            movement.MoveTo(Target.position, Target.rotation);
        }

        private void Update()
        {
            // Update Submodules.
            Input.Update();

            if (!Input.Active)
            {
                HandModel.gameObject.SetActive(false);
                return;
            }
            
            // Update Targets Pose.
            Target.position = Input.Position;
            Target.rotation = Input.Rotation;
            
            if (ActiveBinding)
            {
                // Hide hand model if we are bound to something
                HandModel.gameObject.SetActive(false);
                
                // Pass inputs to bound object.
                ActiveBinding.bindable.InputCallback(this, IVRBindable.InputType.Trigger, Input.Trigger);
            }
            else
            {
                // Show hand and animate if unbound.
                HandModel.gameObject.SetActive(true);
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
        
        public class ChiralData
        {
            public Func<XRController> controller;
            public string nameTemplate;
        }
    }
}