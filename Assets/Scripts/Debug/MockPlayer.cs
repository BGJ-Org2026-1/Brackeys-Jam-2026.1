using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(CapsuleCollider))]
public sealed class MockPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4.0f;
    public bool useXZ = true;

    [Header("Interaction")]
    public Key interactKey = Key.E;
    public float interactRadius = 1.2f;
    public LayerMask interactMask = ~0;
    [SerializeField] private CharacterController controller;
    private void Reset()
    {
        gameObject.tag = "Player";

        var col = GetComponent<CapsuleCollider>();
        col.isTrigger = false;
        col.height = 1.8f;
        col.radius = 0.25f;
        col.center = new Vector3(0f, 0.9f, 0f);

        // Optional, but helps physics feel stable if you later add rigidbody.
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnEnable()
    {
        CameraModeController.OnModeChanged+=ToggleVisibility;
    }

    private void OnDisable()
    {
        
    }

    private void ToggleVisibility(ViewMode mode)
    {
        
    }

    private void Update()
    {
        Move();

        if (Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    private void Move()
    {
        Vector2 v = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) v.x -= 1f;
            if (Keyboard.current.dKey.isPressed) v.x += 1f;
            if (Keyboard.current.sKey.isPressed) v.y -= 1f;
            if (Keyboard.current.wKey.isPressed) v.y += 1f;
        }

        if (v.sqrMagnitude > 1f) v.Normalize();

        Vector3 move = new Vector3(v.x, 0f, v.y);
        controller.Move(move * (moveSpeed * Time.deltaTime));
        
        // float x = Input.GetAxisRaw("Horizontal");
        // float z = Input.GetAxisRaw("Vertical");
        //
        // Vector3 dir = new Vector3(x, 0f, z);
        // if (dir.sqrMagnitude > 1f) dir.Normalize();
        //
        // Vector3 delta = dir * (moveSpeed * Time.deltaTime);
        //
        // if (useXZ)
        // {
        //     transform.position += delta;
        // }
        // else
        // {
        //     transform.position += new Vector3(delta.x, delta.z, 0f);
        // }
    }


    private void TryInteract()
    {
        var hits = Physics.OverlapSphere(transform.position, interactRadius, interactMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            var portal = hits[i].GetComponentInParent<Portal>();
            if (portal == null) continue;

            portal.TryInteract();
            return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
