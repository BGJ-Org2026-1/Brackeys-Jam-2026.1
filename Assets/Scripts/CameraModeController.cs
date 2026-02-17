using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ViewMode { Inside, Inspect }

public sealed class CameraModeController : MonoBehaviour
{
    public static event Action<ViewMode> OnModeChanged;
    public static event Action OnInspectEntered;
    public static event Action OnInspectExited;

    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform insideAnchor;
    [SerializeField] private Transform inspectAnchor;

    [Header("Mode")]
    [SerializeField] private ViewMode startMode = ViewMode.Inside;
    [SerializeField] private bool allowPlayerToggle = true;

    [Header("Input System")]
    [Tooltip("Bind to Gameplay/ToggleInspect (Button).")]
    [SerializeField] private InputActionReference toggleInspectAction;

    [Header("Projection Settings")]
    [SerializeField] private float insideOrthoSize = 6f;
    [SerializeField] private float inspectFieldOfView = 60f;

    [Header("Transition")]
    [SerializeField] private ScreenFader fader;

    [Header("Debug")]
    [SerializeField] private bool logModeChanges = false;

    public ViewMode CurrentMode { get; private set; } = ViewMode.Inside;

    private void Reset()
    {
        targetCamera = Camera.main;
        cameraPivot = transform;
    }

    private void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (!cameraPivot) cameraPivot = transform;

        SetModeImmediate(startMode);
    }

    private void OnEnable()
    {
        if (toggleInspectAction?.action != null)
        {
            toggleInspectAction.action.Enable();
            toggleInspectAction.action.performed += OnTogglePerformed;
        }
    }

    private void OnDisable()
    {
        if (toggleInspectAction?.action != null)
        {
            toggleInspectAction.action.performed -= OnTogglePerformed;
            toggleInspectAction.action.Disable();
        }
    }

    private void OnTogglePerformed(InputAction.CallbackContext _)
    {
        if (!allowPlayerToggle) return;
        ToggleMode();
    }

    [ContextMenu("Toggle Mode")]
    public void ToggleMode()
    {
        SetMode(CurrentMode == ViewMode.Inside ? ViewMode.Inspect : ViewMode.Inside);
    }

    [ContextMenu("Set Inside")]
    public void SetInside() => SetMode(ViewMode.Inside);

    [ContextMenu("Set Inspect")]
    public void SetInspect() => SetMode(ViewMode.Inspect);

    public void SetMode(ViewMode mode)
    {
        if (mode == CurrentMode) return;

        if (fader != null) fader.FadeOutIn(() => ApplyModeImmediate(mode));
        else ApplyModeImmediate(mode);
    }

    public void SetModeImmediate(ViewMode mode)
    {
        ApplyModeImmediate(mode);
    }

    private void ApplyModeImmediate(ViewMode mode)
    {
        CurrentMode = mode;

        var anchor = (mode == ViewMode.Inside) ? insideAnchor : inspectAnchor;
        if (anchor && cameraPivot)
        {
            cameraPivot.position = anchor.position;
            cameraPivot.rotation = anchor.rotation;
        }

        ApplyProjection(mode);
        FireModeEvents(mode);
    }

    private void ApplyProjection(ViewMode mode)
    {
        if (!targetCamera) return;

        if (mode == ViewMode.Inside)
        {
            targetCamera.orthographic = true;
            targetCamera.orthographicSize = insideOrthoSize;
        }
        else
        {
            targetCamera.orthographic = false;
            targetCamera.fieldOfView = inspectFieldOfView;
        }
    }

    private void FireModeEvents(ViewMode mode)
    {
        if (logModeChanges) Debug.Log($"Camera mode now {mode}");

        OnModeChanged?.Invoke(mode);

        if (mode == ViewMode.Inspect) OnInspectEntered?.Invoke();
        else OnInspectExited?.Invoke();
    }
}
