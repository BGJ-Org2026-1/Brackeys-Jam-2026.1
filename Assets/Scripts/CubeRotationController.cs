using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CubeRotationController : MonoBehaviour
{
    public static event Action OnRotationStarted;
    public static event Action OnRotationFinished;
    public static event Action<Quaternion> OnCubeRotationChanged;

    public enum RotationMode { InsideStep, InspectFree }

    [Header("Refs")]
    [SerializeField] private Transform cube;

    [Header("Mode")]
    [SerializeField] private RotationMode mode = RotationMode.InsideStep;
    [SerializeField] private bool autoModeFromCamera = true;

    [Header("Enable")]
    [SerializeField] private bool rotationEnabled = true;

    [Header("Input Inspect")]
    [SerializeField] private InputActionReference mouseDelta;   // Vector2
    [SerializeField] private InputActionReference mouseHold;    // Button
    [SerializeField] private InputActionReference keysRotate;   // Vector2 optional

    [Header("Input Step")]
    [SerializeField] private InputActionReference stepRotate;   // Vector2
    [SerializeField] private InputActionReference stepRoll;     // Axis optional

    [Header("Inspect Feel")]
    [SerializeField] private float mouseDegreesPerPixel = 0.25f;
    [SerializeField] private float mouseBoost = 1.35f;
    [SerializeField] private bool invertX = false;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float keyboardDegreesPerSecond = 120f;

    [Header("Snap Inspect")]
    [SerializeField] private float snapIncrement = 90f;
    [SerializeField] private float snapTime = 0.10f;
    [Tooltip("After keyboard rotation stops, wait this long then snap.")]
    [SerializeField] private float keySnapDelay = 0.08f;

    [Header("Step Inside")]
    [SerializeField] private float stepDegrees = 90f;
    [SerializeField] private float stepRepeatDelay = 0.18f;
    [SerializeField] private float stepLerpTime = 0.10f;

    [Header("Finish")]
    [SerializeField] private float idleToFinish = 0.12f;

    private bool _dragging;
    private bool _active;
    private float _idle;

    private float _stepCooldown;

    private bool _stepLerping;
    private float _stepT;
    private Quaternion _stepFrom;
    private Quaternion _stepTo;

    private bool _snapping;
    private float _snapT;
    private Quaternion _snapFrom;
    private Quaternion _snapTo;

    private bool _keyWasRotating;
    private float _keyIdle;

    private void Reset() => cube = transform;

    private void Awake()
    {
        if (!cube) cube = transform;
    }

    private void OnEnable()
    {
        Enable(mouseDelta); Enable(mouseHold); Enable(keysRotate);
        Enable(stepRotate); Enable(stepRoll);

        if (mouseHold?.action != null)
        {
            mouseHold.action.performed += OnMouseHoldDown;
            mouseHold.action.canceled += OnMouseHoldUp;
        }

        if (autoModeFromCamera)
            CameraModeController.OnModeChanged += OnCameraMode;
    }

    private void OnDisable()
    {
        if (mouseHold?.action != null)
        {
            mouseHold.action.performed -= OnMouseHoldDown;
            mouseHold.action.canceled -= OnMouseHoldUp;
        }

        if (autoModeFromCamera)
            CameraModeController.OnModeChanged -= OnCameraMode;

        Disable(mouseDelta); Disable(mouseHold); Disable(keysRotate);
        Disable(stepRotate); Disable(stepRoll);
    }

    private void OnMouseHoldDown(InputAction.CallbackContext _)
    {
        _dragging = true;
        _snapping = false;
        _keyWasRotating = false;
        _keyIdle = 0f;
    }

    private void OnMouseHoldUp(InputAction.CallbackContext _)
    {
        _dragging = false;
        if (mode == RotationMode.InspectFree) StartSnap();
    }

    private void OnCameraMode(ViewMode v)
    {
        mode = (v == ViewMode.Inspect) ? RotationMode.InspectFree : RotationMode.InsideStep;

        _dragging = false;
        _snapping = false;
        _keyWasRotating = false;
        _keyIdle = 0f;
    }

    private void Update()
    {
        if (!rotationEnabled || !cube) return;

        if (_stepCooldown > 0f) _stepCooldown -= Time.deltaTime;

        if (_stepLerping) { TickLerp(ref _stepT, stepLerpTime, _stepFrom, _stepTo, ref _stepLerping, false); return; }
        if (_snapping) { TickLerp(ref _snapT, snapTime, _snapFrom, _snapTo, ref _snapping, true); return; }

        bool didRotate = false;

        if (mode == RotationMode.InspectFree)
        {
            didRotate |= InspectMouse();
            didRotate |= InspectKeys();
            TickKeyboardSnap();
        }
        else
        {
            didRotate |= StepInput();
        }

        if (didRotate) { BeginIfNeeded(); _idle = 0f; }
        else if (_active)
        {
            _idle += Time.deltaTime;
            if (_idle >= idleToFinish) EndIfActive();
        }
    }

    private bool InspectMouse()
    {
        if (!_dragging) return false;
        if (mouseDelta?.action == null) return false;

        Vector2 d = mouseDelta.action.ReadValue<Vector2>();
        if (d.sqrMagnitude < 0.000001f) return false;

        float sx = invertX ? -1f : 1f;
        float sy = invertY ? -1f : 1f;

        float yaw = d.x * mouseDegreesPerPixel * mouseBoost * sx;
        float pitch = -d.y * mouseDegreesPerPixel * mouseBoost * sy;

        cube.Rotate(Vector3.up, yaw, Space.World);
        cube.Rotate(Vector3.right, pitch, Space.World);

        OnCubeRotationChanged?.Invoke(cube.rotation);
        BeginIfNeeded();
        _idle = 0f;
        return true;
    }

    private bool InspectKeys()
    {
        if (keysRotate?.action == null) return false;

        Vector2 v = keysRotate.action.ReadValue<Vector2>();
        if (v.sqrMagnitude < 0.000001f) return false;

        float yaw = v.x * keyboardDegreesPerSecond * Time.deltaTime;
        float pitch = v.y * keyboardDegreesPerSecond * Time.deltaTime;

        cube.Rotate(Vector3.up, yaw, Space.World);
        cube.Rotate(Vector3.right, -pitch, Space.World);

        OnCubeRotationChanged?.Invoke(cube.rotation);
        BeginIfNeeded();
        _idle = 0f;

        _keyWasRotating = true;
        _keyIdle = 0f;

        return true;
    }

    private void TickKeyboardSnap()
    {
        if (_dragging) return;
        if (!_keyWasRotating) return;

        _keyIdle += Time.deltaTime;
        if (_keyIdle < keySnapDelay) return;

        _keyWasRotating = false;
        _keyIdle = 0f;

        StartSnap();
    }

    private bool StepInput()
    {
        if (_stepCooldown > 0f) return false;

        Vector2 v = (stepRotate?.action != null) ? stepRotate.action.ReadValue<Vector2>() : Vector2.zero;
        float roll = (stepRoll?.action != null) ? stepRoll.action.ReadValue<float>() : 0f;

        int sx = SignStep(v.x);
        int sy = SignStep(v.y);
        int sr = SignStep(roll);

        if (sx == 0 && sy == 0 && sr == 0) return false;

        _stepCooldown = stepRepeatDelay;

        Vector3 axis;
        float deg;

        if (sx != 0) { axis = Vector3.up; deg = stepDegrees * sx; }
        else if (sy != 0) { axis = Vector3.right; deg = -stepDegrees * sy; }
        else { axis = Vector3.forward; deg = -stepDegrees * sr; }

        _stepFrom = cube.rotation;
        _stepTo = Quaternion.AngleAxis(deg, axis) * _stepFrom;
        _stepT = 0f;
        _stepLerping = true;

        BeginIfNeeded();
        return true;
    }

    private void StartSnap()
    {
        if (mode != RotationMode.InspectFree) return;

        _snapFrom = cube.rotation;
        _snapTo = SnapRotation(_snapFrom, snapIncrement);

        if (Quaternion.Angle(_snapFrom, _snapTo) < 0.1f)
        {
            cube.rotation = _snapTo;
            OnCubeRotationChanged?.Invoke(cube.rotation);
            EndIfActive();
            return;
        }

        _snapT = 0f;
        _snapping = true;
        BeginIfNeeded();
    }

    private void TickLerp(ref float t, float time, Quaternion from, Quaternion to, ref bool flag, bool finishEnds)
    {
        t += Time.deltaTime / Mathf.Max(0.0001f, time);
        float a = Mathf.Clamp01(t);

        cube.rotation = Quaternion.Slerp(from, to, a);
        OnCubeRotationChanged?.Invoke(cube.rotation);

        if (a >= 1f)
        {
            flag = false;
            cube.rotation = to;
            OnCubeRotationChanged?.Invoke(cube.rotation);
            if (finishEnds) EndIfActive();
        }
    }

    private void BeginIfNeeded()
    {
        if (_active) return;
        _active = true;
        OnRotationStarted?.Invoke();
    }

    private void EndIfActive()
    {
        if (!_active) return;
        _active = false;
        _idle = 0f;
        OnRotationFinished?.Invoke();
    }

    private static int SignStep(float v)
    {
        if (v > 0.5f) return 1;
        if (v < -0.5f) return -1;
        return 0;
    }

    private static Quaternion SnapRotation(Quaternion q, float inc)
    {
        Vector3 e = q.eulerAngles;
        e.x = SnapAngle(e.x, inc);
        e.y = SnapAngle(e.y, inc);
        e.z = SnapAngle(e.z, inc);
        return Quaternion.Euler(e);
    }

    private static float SnapAngle(float a, float inc)
    {
        if (inc <= 0.0001f) return a;
        float s = Mathf.Round(a / inc) * inc;
        return Mathf.Repeat(s, 360f);
    }

    private static void Enable(InputActionReference r) { if (r?.action != null) r.action.Enable(); }
    private static void Disable(InputActionReference r) { if (r?.action != null) r.action.Disable(); }
}
