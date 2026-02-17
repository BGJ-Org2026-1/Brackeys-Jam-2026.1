using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class MockPlayerClimber : MonoBehaviour
{
    [Header("Refs")]
    public MockRotationController rotation;

    [Header("Movement")]
    public float moveSpeed = 4.0f;

    [Header("Current Surface")]
    [SerializeField] private FaceName currentFloorFace = FaceName.Bottom;

    private CharacterController controller;
    
    private Vector2 ReadMove()
    {
        Vector2 v = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) v.x -= 1f;
            if (Keyboard.current.dKey.isPressed) v.x += 1f;
            if (Keyboard.current.sKey.isPressed) v.y -= 1f;
            if (Keyboard.current.wKey.isPressed) v.y += 1f;
        }

        if (v.sqrMagnitude > 1f) v.Normalize();
        return v;
    }


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.slopeLimit = 90f;
        controller.stepOffset = 0f;
    }

    private void OnEnable()
    {
        if (rotation != null)
        {
            rotation.OnRotationSnapped += HandleRotationSnapped;
            rotation.OnInspectExited += HandleInspectExited;
        }
    }

    private void OnDisable()
    {
        if (rotation != null)
        {
            rotation.OnRotationSnapped -= HandleRotationSnapped;
            rotation.OnInspectExited -= HandleInspectExited;
        }
    }

    private void Update()
    {
        Vector2 v = ReadMove();
        Vector3 move = ComputeSurfaceMove(v.x, v.y);
        controller.Move(move * (moveSpeed * Time.deltaTime));
    }

    private void HandleRotationSnapped(FaceName floorFace)
    {
        currentFloorFace = floorFace;
    }

    private void HandleInspectExited(FaceName floorFace)
    {
        currentFloorFace = floorFace;
    }

    private Vector3 ComputeSurfaceMove(float inputA, float inputB)
    {
        // Map inputs onto a plane based on which face is currently "floor".
        // This is a mock. Real player should follow wall tangents and gravity rules.
        // Here we just pick two axes that form the plane, and ignore the normal axis.

        switch (currentFloorFace)
        {
            // Floor is Bottom or Top, walk on XZ plane
            case FaceName.Bottom:
            case FaceName.Top:
                return new Vector3(inputA, 0f, inputB);

            // Floor is Front or Back, walk on XY plane
            case FaceName.Front:
            case FaceName.Back:
                return new Vector3(inputA, inputB, 0f);

            // Floor is Left or Right, walk on YZ plane
            case FaceName.Left:
            case FaceName.Right:
                return new Vector3(0f, inputB, inputA);

            default:
                return new Vector3(inputA, 0f, inputB);
        }
    }
}
