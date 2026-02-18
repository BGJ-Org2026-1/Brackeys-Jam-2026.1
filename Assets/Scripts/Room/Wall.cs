using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class Wall : MonoBehaviour
{
    [Header("Identity")]
    public FaceName faceName;
    public bool isInterior = true;

    [Header("Gameplay")]
    public bool isWalkable = false;

    [Header("Audio")]
    public AudioClip footstepClip;

    private readonly List<ItemPickup> items = new List<ItemPickup>();
    private readonly List<Portal> portals = new List<Portal>();

    public IReadOnlyList<ItemPickup> Items => items;
    public IReadOnlyList<Portal> Portals => portals;

    public void RebuildCache()
    {
        items.Clear();
        portals.Clear();

        GetComponentsInChildren(true, items);
        GetComponentsInChildren(true, portals);
    }

    public void SetContentActive(bool active)
    {
        for (int i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (it != null) it.gameObject.SetActive(active);
        }

        for (int i = 0; i < portals.Count; i++)
        {
            var p = portals[i];
            if (p != null) p.gameObject.SetActive(active);
        }
    }
}