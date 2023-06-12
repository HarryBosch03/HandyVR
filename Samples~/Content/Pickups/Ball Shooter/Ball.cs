using System;
using UnityEngine;

    
[SelectionBase]
[DisallowMultipleComponent]
public sealed class Ball : MonoBehaviour
{
    [SerializeField] private float maxDistance = 50.0f;

    private new Rigidbody rigidbody;
    private Vector3 returnPosition;

    private void Awake()
    {
        returnPosition = transform.position;
    }

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!((transform.position - returnPosition).magnitude > maxDistance)) return;
        
        transform.position = returnPosition;
        rigidbody.AddForce(-rigidbody.velocity, ForceMode.VelocityChange);
        rigidbody.AddTorque(-rigidbody.angularVelocity, ForceMode.VelocityChange);
    }
}
