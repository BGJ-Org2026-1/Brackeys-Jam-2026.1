using System.Collections.Generic;
using UnityEngine;

public sealed class MockInventory : MonoBehaviour
{
    [SerializeField] private List<string> startingItems = new List<string>();
    private readonly HashSet<string> items = new HashSet<string>();

    private void Awake()
    {
        items.Clear();
        for (int i = 0; i < startingItems.Count; i++)
        {
            var id = startingItems[i];
            if (!string.IsNullOrWhiteSpace(id)) items.Add(id);
        }
    }
    private void OnEnable()
    {
        PuzzleBase.OnRewardItemGranted += Add;
    }

    private void OnDisable()
    {
        PuzzleBase.OnRewardItemGranted -= Add;
    }
    

    public bool Has(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return true;
        return items.Contains(itemId);
    }

    public void Add(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;
        items.Add(itemId);
    }
}