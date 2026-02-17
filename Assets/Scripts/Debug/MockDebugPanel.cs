using UnityEngine;

public sealed class MockDebugPanel : MonoBehaviour
{
    public RoomManager roomManager;
    public MockRotationController rotation;


    [ContextMenu("Load Room 0")]
    public void LoadRoom0()
    {
        if (roomManager != null) roomManager.LoadRoomByIndex(0);
    }



    [ContextMenu("Rotate X 90")]
    public void RX90()
    {
        if (rotation != null) rotation.RotateX90();
    }

    [ContextMenu("Rotate Y 90")]
    public void RY90()
    {
        if (rotation != null) rotation.RotateY90();
    }

    [ContextMenu("Rotate Z 90")]
    public void RZ90()
    {
        if (rotation != null) rotation.RotateZ90();
    }
    
}