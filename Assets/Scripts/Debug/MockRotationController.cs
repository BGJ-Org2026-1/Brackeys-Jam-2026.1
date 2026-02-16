using System;
using UnityEngine;

public sealed class MockRotationController : MonoBehaviour
{
    [Header("Target")]
    public Transform roomRoot;

    [Header("Timing")]
    public float snapDuration = 0.25f;

    [Header("Inspect Free Rotate")]
    public bool inspectMode;
    public int mouseButton = 1;
    public float freeRotateSpeed = 180f;

    public event Action OnRotationStarted;
    public event Action<FaceName> OnRotationSnapped;
    public event Action OnInspectEntered;
    public event Action<FaceName> OnInspectExited;

    private bool isSnapping;
    private Quaternion fromRot;
    private Quaternion toRot;
    private float t;

    private Orientation orientation = Orientation.Identity();

    private void Update()
    {
        if (roomRoot == null) return;

        if (inspectMode && !isSnapping)
        {
            UpdateFreeRotate();
        }

        if (isSnapping)
        {
            UpdateSnap();
        }
    }

    public void SetRoomRoot(Transform tRoot)
    {
        roomRoot = tRoot;
        orientation = Orientation.Identity();
    }

    [ContextMenu("MOCK Enter Inspect")]
    public void EnterInspect()
    {
        if (inspectMode) return;
        inspectMode = true;
        OnInspectEntered?.Invoke();
    }

    [ContextMenu("MOCK Exit Inspect Snap")]
    public void ExitInspect()
    {
        if (!inspectMode) return;
        inspectMode = false;
        SnapToNearestNinety();
        OnInspectExited?.Invoke(orientation.Down);
    }

    [ContextMenu("Rotate X 90")]
    public void RotateX90()
    {
        orientation = orientation.RotateX90();
        StartSnap(roomRoot.rotation * Quaternion.Euler(90f, 0f, 0f));
    }

    [ContextMenu("Rotate X 270")]
    public void RotateX270()
    {
        orientation = orientation.RotateX270();
        StartSnap(roomRoot.rotation * Quaternion.Euler(270f, 0f, 0f));
    }

    [ContextMenu("Rotate Y 90")]
    public void RotateY90()
    {
        orientation = orientation.RotateY90();
        StartSnap(roomRoot.rotation * Quaternion.Euler(0f, 90f, 0f));
    }

    [ContextMenu("Rotate Y 270")]
    public void RotateY270()
    {
        orientation = orientation.RotateY270();
        StartSnap(roomRoot.rotation * Quaternion.Euler(0f, 270f, 0f));
    }

    [ContextMenu("Rotate Z 90")]
    public void RotateZ90()
    {
        orientation = orientation.RotateZ90();
        StartSnap(roomRoot.rotation * Quaternion.Euler(0f, 0f, 90f));
    }

    [ContextMenu("Rotate Z 270")]
    public void RotateZ270()
    {
        orientation = orientation.RotateZ270();
        StartSnap(roomRoot.rotation * Quaternion.Euler(0f, 0f, 270f));
    }

    [ContextMenu("Snap To Nearest 90")]
    public void SnapToNearestNinety()
    {
        if (roomRoot == null) return;

        Vector3 e = roomRoot.rotation.eulerAngles;
        e.x = RoundToNinety(e.x);
        e.y = RoundToNinety(e.y);
        e.z = RoundToNinety(e.z);

        StartSnap(Quaternion.Euler(e));
    }

    private void UpdateFreeRotate()
    {
        if (!Input.GetMouseButton(mouseButton)) return;

        float dx = Input.GetAxis("Mouse X");
        float dy = Input.GetAxis("Mouse Y");

        Quaternion yaw = Quaternion.AngleAxis(dx * freeRotateSpeed * Time.unscaledDeltaTime, Vector3.up);
        Quaternion pitch = Quaternion.AngleAxis(dy * freeRotateSpeed * Time.unscaledDeltaTime, Vector3.right);

        roomRoot.rotation = yaw * pitch * roomRoot.rotation;
    }

    private void StartSnap(Quaternion target)
    {
        if (roomRoot == null) return;

        OnRotationStarted?.Invoke();

        isSnapping = true;
        fromRot = roomRoot.rotation;
        toRot = target;
        t = 0f;
    }

    private void UpdateSnap()
    {
        t += Time.unscaledDeltaTime;

        float u = snapDuration <= 0f ? 1f : Mathf.Clamp01(t / snapDuration);
        roomRoot.rotation = Quaternion.Slerp(fromRot, toRot, u);

        if (u >= 1f)
        {
            isSnapping = false;
            OnRotationSnapped?.Invoke(orientation.Down);
        }
    }

    private static float RoundToNinety(float degrees)
    {
        return Mathf.Round(degrees / 90f) * 90f;
    }

    private struct Orientation
    {
        public FaceName Up;
        public FaceName Down;
        public FaceName Forward;
        public FaceName Back;
        public FaceName Left;
        public FaceName Right;

        public static Orientation Identity()
        {
            return new Orientation
            {
                Up = FaceName.Top,
                Down = FaceName.Bottom,
                Forward = FaceName.Front,
                Back = FaceName.Back,
                Left = FaceName.Left,
                Right = FaceName.Right
            };
        }

        public Orientation RotateX90()
        {
            return new Orientation
            {
                Up = Forward,
                Down = Back,
                Forward = Down,
                Back = Up,
                Left = Left,
                Right = Right
            };
        }

        public Orientation RotateX270()
        {
            return new Orientation
            {
                Up = Back,
                Down = Forward,
                Forward = Up,
                Back = Down,
                Left = Left,
                Right = Right
            };
        }

        public Orientation RotateY90()
        {
            return new Orientation
            {
                Up = Up,
                Down = Down,
                Forward = Left,
                Back = Right,
                Left = Back,
                Right = Forward
            };
        }

        public Orientation RotateY270()
        {
            return new Orientation
            {
                Up = Up,
                Down = Down,
                Forward = Right,
                Back = Left,
                Left = Forward,
                Right = Back
            };
        }

        public Orientation RotateZ90()
        {
            return new Orientation
            {
                Up = Right,
                Down = Left,
                Forward = Forward,
                Back = Back,
                Left = Up,
                Right = Down
            };
        }

        public Orientation RotateZ270()
        {
            return new Orientation
            {
                Up = Left,
                Down = Right,
                Forward = Forward,
                Back = Back,
                Left = Down,
                Right = Up
            };
        }
    }
}
