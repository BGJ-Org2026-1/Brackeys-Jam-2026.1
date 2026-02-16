using UnityEngine;

public sealed class RoomRotationAndFloorBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RoomManager roomManager;

    [Tooltip("This should be the same transform being rotated by CubeRotationController. Usually RoomManager.roomContainer.")]
    [SerializeField] private Transform cubeRoot;

    private void OnEnable()
    {
        if (roomManager != null)
        {
            roomManager.OnRoomLoaded += HandleRoomLoaded;
        }

        CubeRotationController.OnRotationFinished += HandleRotationFinished;
        CameraModeController.OnInspectExited += HandleInspectExited;
    }

    private void OnDisable()
    {
        if (roomManager != null)
        {
            roomManager.OnRoomLoaded -= HandleRoomLoaded;
        }

        CubeRotationController.OnRotationFinished -= HandleRotationFinished;
        CameraModeController.OnInspectExited -= HandleInspectExited;
    }

    private void HandleRoomLoaded(Room room)
    {
        ApplyFloorToCurrentRoom();
    }

    private void HandleRotationFinished()
    {
        ApplyFloorToCurrentRoom();
    }

    private void HandleInspectExited()
    {
        ApplyFloorToCurrentRoom();
    }

    [ContextMenu("Apply Floor Now")]
    public void ApplyFloorToCurrentRoom()
    {
        if (roomManager == null) return;
        var room = roomManager.CurrentRoom;
        if (room == null) return;

        if (cubeRoot == null)
        {
            // Best guess, use room root if you did not assign it.
            cubeRoot = room.transform;
        }

        FaceName floor = FloorFaceResolver.GetFloorFace(cubeRoot);
        room.ApplyFloorFace(floor);
    }
}