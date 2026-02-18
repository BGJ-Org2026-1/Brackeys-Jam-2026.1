using UnityEngine;

public sealed class BillboardSprite : MonoBehaviour
{
    public Camera targetCamera;
    public bool lockPitch = true;
    public bool invertForward = false;

    private void LateUpdate()
    {
        var cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        Vector3 toCam = cam.transform.position - transform.position;
        if (lockPitch) toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.000001f) return;

        Quaternion look = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        if (invertForward) look *= Quaternion.Euler(0f, 180f, 0f);

        transform.rotation = look;
    }
}