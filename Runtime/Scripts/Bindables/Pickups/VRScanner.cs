using HandyVR.Interfaces;
using HandyVR.Player;
using HandyVR.Player.Input;
using HandyVR.Switches;
using TMPro;
using UnityEngine;

namespace HandyVR.Bindables.Pickups
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class VRScanner : MonoBehaviour, IVRBindableListener
    {
        [SerializeField] private Transform scannerTarget;
        [SerializeField] private float radius = 0.1f;

        [Space]
        [SerializeField] private string template;
        [SerializeField] private TMP_Text uiText;
        [SerializeField] private TMP_Text nameText;

        [Space] 
        [SerializeField] private ParticleSystem scannerFX;

        public void InputCallback(VRHand hand, VRBindable bindable, IVRBindable.InputType type, HandInput.InputWrapper input)
        {
            switch (type)
            {
                case IVRBindable.InputType.Trigger:
                    Scan(input);
                    break;
                default:
                    return;
            }
        }

        private void Scan(HandInput.InputWrapper input)
        {
            uiText.text = string.Empty;
            nameText.text = string.Empty;

            scannerFX.SyncPlayState(input.Down);
            
            if (!input.Down) return;
            
            var ray = new Ray(scannerTarget.position, scannerTarget.forward);
            if (!Physics.SphereCast(ray, radius, out var hit)) return;

            nameText.text = hit.transform.name;

            var driver = hit.transform.GetComponentInParent<FloatDriver>();
            if (!driver) return;

            uiText.text = string.Format(template, Mathf.Round(driver.Value * 100.0f) / 100.0f);
        }
    }
}
