using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Room : MonoBehaviour
{
    [Header("Contract Roots")]
    [SerializeField] private Transform geometryRoot;
    [SerializeField] private Transform anchorsRoot;
    [SerializeField] private Transform puzzlesRoot; // optional, points to "Puzzles"

    [Header("Cached")]
    [SerializeField] private Transform defaultSpawn;

    private readonly Dictionary<FaceName, Wall> interiorWalls = new Dictionary<FaceName, Wall>();
    private readonly Dictionary<FaceName, Wall> exteriorWalls = new Dictionary<FaceName, Wall>();

    private readonly List<Wall> allWalls = new List<Wall>();
    private readonly List<ItemPickup> allItems = new List<ItemPickup>();
    private readonly List<Portal> allPortals = new List<Portal>();
    private readonly List<PuzzleBase> allPuzzles = new List<PuzzleBase>();

    public event Action<Room> OnRoomInitialized;
    public event Action<Room> OnRoomActivated;
    public event Action<Room> OnRoomDeactivated;

    public Transform DefaultSpawn => defaultSpawn;

    public IReadOnlyList<Wall> AllWalls => allWalls;
    public IReadOnlyList<ItemPickup> AllItems => allItems;
    public IReadOnlyList<Portal> AllPortals => allPortals;
    public IReadOnlyList<PuzzleBase> AllPuzzles => allPuzzles;

    public void Initialize()
    {
        AutoWireRoots();
        CacheAnchors();
        CacheWallsAndFlatten();
        CacheRoomPuzzles();

        OnRoomInitialized?.Invoke(this);
    }

    public void Activate() => OnRoomActivated?.Invoke(this);
    public void Deactivate() => OnRoomDeactivated?.Invoke(this);

    public Wall GetInteriorWall(FaceName face)
    {
        interiorWalls.TryGetValue(face, out var w);
        return w;
    }

    public Wall GetExteriorWall(FaceName face)
    {
        exteriorWalls.TryGetValue(face, out var w);
        return w;
    }

    public Transform GetAnchor(string name)
    {
        if (anchorsRoot == null) return null;
        if (string.IsNullOrWhiteSpace(name)) name = "Spawn";
        return anchorsRoot.Find(name);
    }

    public void ApplyFloorFace(FaceName floorFace)
    {
        for (int i = 0; i < allWalls.Count; i++)
        {
            var w = allWalls[i];
            if (w == null) continue;
            if (!w.isInterior) continue;

            bool enable = w.isWalkable;
            w.SetContentActive(enable);
        }

        var floorWall = GetInteriorWall(floorFace);
        if (floorWall != null)
        {
            floorWall.SetContentActive(true);
        }
    }

    private void AutoWireRoots()
    {
        if (geometryRoot == null) geometryRoot = transform.Find("Geometry");
        if (anchorsRoot == null) anchorsRoot = transform.Find("Anchors");
        if (puzzlesRoot == null) puzzlesRoot = transform.Find("Puzzles");
    }

    private void CacheAnchors()
    {
        if (anchorsRoot == null) return;
        if (defaultSpawn == null) defaultSpawn = anchorsRoot.Find("Spawn");
    }

    private void CacheWallsAndFlatten()
    {
        interiorWalls.Clear();
        exteriorWalls.Clear();

        allWalls.Clear();
        allItems.Clear();
        allPortals.Clear();
        allPuzzles.Clear();

        if (geometryRoot == null) return;

        var interior = geometryRoot.Find("Interior");
        var exterior = geometryRoot.Find("Exterior");

        CacheWallGroup(interior, true, interiorWalls);
        CacheWallGroup(exterior, false, exteriorWalls);

        for (int i = 0; i < allWalls.Count; i++)
        {
            var w = allWalls[i];
            if (w == null) continue;

            // Items
            var wItems = w.Items;
            for (int j = 0; j < wItems.Count; j++)
            {
                var it = wItems[j];
                if (it != null) allItems.Add(it);
            }

            // Portals
            var wPortals = w.Portals;
            for (int j = 0; j < wPortals.Count; j++)
            {
                var p = wPortals[j];
                if (p != null) allPortals.Add(p);
            }
            
            // var wPuzzles = w.Puzzles;
            // for (int j = 0; j < wPuzzles.Count; j++)
            // {
            //     var z = wPuzzles[j];
            //     if (z != null) allPuzzles.Add(z);
            // }
        }
    }

    private void CacheRoomPuzzles()
    {
        // Adds puzzles under "Puzzles" root that are not under walls
        if (puzzlesRoot == null) return;

        var roomPuzzles = puzzlesRoot.GetComponentsInChildren<PuzzleBase>(true);
        for (int i = 0; i < roomPuzzles.Length; i++)
        {
            var z = roomPuzzles[i];
            if (z == null) continue;

            if (!allPuzzles.Contains(z))
            {
                allPuzzles.Add(z);
            }
        }
    }

    private void CacheWallGroup(Transform group, bool isInterior, Dictionary<FaceName, Wall> dict)
    {
        if (group == null) return;

        var walls = group.GetComponentsInChildren<Wall>(true);
        for (int i = 0; i < walls.Length; i++)
        {
            var w = walls[i];
            if (w == null) continue;

            w.isInterior = isInterior;
            w.RebuildCache();

            dict[w.faceName] = w;
            allWalls.Add(w);
        }
    }
}
