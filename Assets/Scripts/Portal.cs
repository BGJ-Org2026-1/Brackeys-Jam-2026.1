using System;
using UnityEngine;

public sealed class Portal : MonoBehaviour
{
    public int targetRoomIndex;
    public string spawnName = "Spawn";
    public bool requireInteraction;
    public string requiredItemId = "";

    public static event Action<Portal, int, string, string> OnPortalTriggered;

    private bool playerInside;

    private void Reset()
    {
        var c3 = GetComponent<Collider>();
        if (c3 != null) c3.isTrigger = true;

        var c2 = GetComponent<Collider2D>();
        if (c2 != null) c2.isTrigger = true;
    }

    public void TryInteract()
    {
        if (!requireInteraction) return;
        if (!playerInside) return;
        Fire();
    }

    private void Fire()
    {
        OnPortalTriggered?.Invoke(this, targetRoomIndex, spawnName, requiredItemId);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (!requireInteraction) Fire();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        if (!requireInteraction) Fire();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }
}