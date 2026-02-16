using System.Collections.Generic;
using UnityEngine;

public sealed class WallVisibilityController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private Camera insideCamera;

    [Header("Dot Sensor")]
    [SerializeField] private float facingThreshold = 0.05f;

    [Header("What to toggle")]
    [SerializeField] private bool toggleWallRenderers = true;
    [SerializeField] private bool toggleWallChildren = true;
    [SerializeField] private bool excludeWallObjectFromChildrenToggle = true;

    private readonly List<Renderer> _rendererCache = new List<Renderer>(64);

    private ViewMode _mode = ViewMode.Inside;
    private bool _rotationInProgress;
    private bool _refreshQueued;

    private void OnEnable()
    {
        CameraModeController.OnInspectEntered += OnInspectEntered;
        CameraModeController.OnInspectExited += OnInspectExited;

        CubeRotationController.OnRotationStarted += OnRotationStarted;
        CubeRotationController.OnRotationFinished += OnRotationFinished;

        if (roomManager != null)
            roomManager.OnRoomLoaded += OnRoomLoaded;
    }

    private void OnDisable()
    {
        CameraModeController.OnInspectEntered -= OnInspectEntered;
        CameraModeController.OnInspectExited -= OnInspectExited;

        CubeRotationController.OnRotationStarted -= OnRotationStarted;
        CubeRotationController.OnRotationFinished -= OnRotationFinished;

        if (roomManager != null)
            roomManager.OnRoomLoaded -= OnRoomLoaded;
    }

    private void OnInspectEntered()
    {
        _mode = ViewMode.Inspect;
        ApplyOutsideVisibility();
    }

    private void OnInspectExited()
    {
        _mode = ViewMode.Inside;
        QueueOrRefreshNow();
    }

    private void OnRoomLoaded(Room _)
    {
        QueueOrRefreshNow();
    }

    private void OnRotationStarted()
    {
        _rotationInProgress = true;
    }

    private void OnRotationFinished()
    {
        _rotationInProgress = false;

        if (_refreshQueued)
        {
            _refreshQueued = false;
            RefreshNow();
        }
        else
        {
            RefreshNow();
        }
    }

    private void QueueOrRefreshNow()
    {
        if (_mode == ViewMode.Inspect) return;

        if (_rotationInProgress)
        {
            _refreshQueued = true;
            return;
        }

        RefreshNow();
    }

    private void ApplyOutsideVisibility()
    {
        var room = roomManager != null ? roomManager.CurrentRoom : null;
        if (room == null || room.AllWalls == null) return;

        foreach (var wall in room.AllWalls)
        {
            if (wall == null) continue;
            SetWallAndChildrenVisible(wall, !wall.isInterior);
        }
    }

    private void RefreshNow()
    {
        if (_mode == ViewMode.Inspect) return;

        var room = roomManager != null ? roomManager.CurrentRoom : null;
        if (room == null || room.AllWalls == null || insideCamera == null) return;

        Vector3 camPos = insideCamera.transform.position;

        foreach (var w in room.AllWalls)
        {
            if (w == null) continue;

            if (!w.isInterior)
            {
                // Exterior hidden in inside mode (matches your existing logic).
                SetWallAndChildrenVisible(w, false);
                continue;
            }

            Transform t = w.transform;
            Vector3 wallNormal = -t.forward;

            Vector3 toCam = camPos - t.position;
            float len = toCam.magnitude;
            toCam = (len > 0.0001f) ? (toCam / len) : Vector3.forward;

            bool facesCamera = Vector3.Dot(wallNormal, toCam) > facingThreshold;
            SetWallAndChildrenVisible(w, facesCamera);
        }
    }

    private void SetWallAndChildrenVisible(Wall wall, bool visible)
    {
        if (wall == null) return;

        if (toggleWallRenderers)
            SetRenderersVisible(wall.gameObject, visible, excludeRoot: false);

        if (toggleWallChildren)
            SetRenderersVisible(wall.gameObject, visible, excludeRoot: excludeWallObjectFromChildrenToggle);
    }

    private void SetRenderersVisible(GameObject root, bool visible, bool excludeRoot)
    {
        if (root == null) return;

        _rendererCache.Clear();
        root.GetComponentsInChildren(true, _rendererCache);

        Transform rootT = root.transform;

        for (int i = 0; i < _rendererCache.Count; i++)
        {
            var r = _rendererCache[i];
            if (r == null) continue;

            if (excludeRoot && r.transform == rootT) continue;

            r.enabled = visible;
        }

        _rendererCache.Clear();
    }
}
