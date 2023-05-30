using HandyVR.Interfaces;
using UnityEngine;

namespace HandyVR.Player.Hands
{
    /// <summary>
    /// Submodule for animating hand.
    /// </summary>
    [SelectionBase]
    [DisallowMultipleComponent]
    public class VRHandAnimator : MonoBehaviour, IVRHandModule
    {
        [Tooltip("Amount of smoothing to apply to inputs. Zero == no smooth")]
        [SerializeField] private float smoothing = 0.1f;

        private static readonly int Trigger = Animator.StringToHash("trigger");
        private static readonly int Grip = Animator.StringToHash("grip");

        private float gripValue, triggerValue;
        
        private VRHand hand;
        private Animator animator;

        public void Init(VRHand hand)
        {
            this.hand = hand;
            animator = GetComponentInChildren<Animator>();
        }

        public void Update()
        {
            float tGripValue, tTriggerValue;

            var bindingController = hand.BindingController as VRHandBinding;
         
            // Matches pointing pose if an object is being pointed at.
            if (bindingController.PointingAt != null)
            {
                tTriggerValue = 0.0f;
                tGripValue = 1.0f;
            }
            else
            {
                tGripValue = hand.Input.Grip.Value;
                tTriggerValue = hand.Input.Trigger.Value;
            }

            // Move actual animator value towards target, using the smoothing value.
            gripValue += smoothing > 0.0f ? (tGripValue - gripValue) / smoothing * Time.deltaTime : tGripValue;
            triggerValue += smoothing > 0.0f ? (tTriggerValue - triggerValue) / smoothing * Time.deltaTime : tTriggerValue;
            
            animator.SetFloat(Grip, gripValue);
            animator.SetFloat(Trigger, triggerValue);
        }
    }
}