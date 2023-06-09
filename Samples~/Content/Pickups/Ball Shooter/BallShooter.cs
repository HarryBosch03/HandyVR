using System;
using HandyVR.Bindables;
using HandyVR.Bindables.Targets;
using UnityEngine;
using HandyVR.Interfaces;
using HandyVR.Player;
using HandyVR.Player.Input;

[SelectionBase]
[DisallowMultipleComponent]
public sealed class BallShooter : MonoBehaviour, IVRBindableListener
{
    [SerializeField] private float shootForce;
    
    private VRSocket socket;

    private void Awake()
    {
        socket = GetComponentInChildren<VRSocket>();
    }

    public void InputCallback(VRHand hand, VRBindable bindable, IVRBindable.InputType type, HandInput.InputWrapper input)
    {
        if (type != IVRBindable.InputType.Trigger) return;
        if (!input.Down || !socket || !socket.ActiveBinding) return;
        
        Shoot();
    }

    private void Shoot()
    {
        var rb = socket.ActiveBinding.bindable.Rigidbody;
        if (!rb) return;
        
        socket.ActiveBinding.Deactivate();
        rb.AddForce(transform.forward * shootForce, ForceMode.Impulse);
    }
}
