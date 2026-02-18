using System;
using UnityEngine;

public sealed class ItemPickup : MonoBehaviour
{
    public string itemId = "Key_01";
    public static event Action<string> OnItemCollected;

    private void Reset()
    {
        var c3 = GetComponent<Collider>();
        if (c3 != null) c3.isTrigger = true;

        var c2 = GetComponent<Collider2D>();
        if (c2 != null) c2.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnItemCollected?.Invoke(itemId);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OnItemCollected?.Invoke(itemId);
        gameObject.SetActive(false);
    }
}