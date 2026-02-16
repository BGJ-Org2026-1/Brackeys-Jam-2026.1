using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class WallVisual : MonoBehaviour
{
    [Header("Renderer Sources")]
    [SerializeField] private Renderer[] renderers;

    [Header("Fade Settings")]
    [SerializeField] private float defaultFadeTime = 0.12f;

    [Tooltip("Disable renderers when fully invisible. Colliders stay active.")]
    [SerializeField] private bool disableRenderersAtZero = true;

    [Header("Shader Color Properties")]
    [Tooltip("Most shaders use one of these. URP Lit often uses _BaseColor, built in often uses _Color.")]
    [SerializeField] private string[] colorPropertyNames = new[] { "_BaseColor", "_Color" };

    private Coroutine fadeRoutine;
    private float currentAlpha = 1f;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private void Reset()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        ApplyAlphaInstant(currentAlpha);
    }

    public void SetVisible(bool visible, float fadeTime = -1f)
    {
        float target = visible ? 1f : 0f;
        FadeTo(target, fadeTime);
    }

    public void FadeTo(float targetAlpha01, float fadeTime = -1f)
    {
        if (fadeTime < 0f) fadeTime = defaultFadeTime;

        targetAlpha01 = Mathf.Clamp01(targetAlpha01);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha01, Mathf.Max(0.0001f, fadeTime)));
    }

    public void ApplyAlphaInstant(float alpha01)
    {
        currentAlpha = Mathf.Clamp01(alpha01);

        if (renderers == null) return;

        bool shouldEnableRenderers = !disableRenderersAtZero || currentAlpha > 0.001f;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            r.enabled = shouldEnableRenderers;

            var block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);

            ApplyColorAlpha(block, r, currentAlpha);

            r.SetPropertyBlock(block);
        }
    }

    private IEnumerator FadeRoutine(float target, float time)
    {
        float start = currentAlpha;

        bool shouldEnableRenderers = true;
        if (disableRenderersAtZero)
        {
            shouldEnableRenderers = target > 0.001f || start > 0.001f;
        }

        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].enabled = shouldEnableRenderers;
            }
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float a = Mathf.Lerp(start, target, Mathf.Clamp01(t));
            ApplyAlphaInstant(a);
            yield return null;
        }

        ApplyAlphaInstant(target);

        if (disableRenderersAtZero && target <= 0.001f && renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].enabled = false;
            }
        }

        fadeRoutine = null;
    }

    private void ApplyColorAlpha(MaterialPropertyBlock block, Renderer r, float alpha)
    {
        if (r == null) return;

        for (int i = 0; i < colorPropertyNames.Length; i++)
        {
            string prop = colorPropertyNames[i];
            if (string.IsNullOrWhiteSpace(prop)) continue;

            int id = prop == "_BaseColor" ? BaseColorId : (prop == "_Color" ? ColorId : Shader.PropertyToID(prop));

            if (!r.sharedMaterial) continue;
            if (!r.sharedMaterial.HasProperty(id)) continue;

            Color c = r.sharedMaterial.GetColor(id);
            c.a = alpha;
            block.SetColor(id, c);
        }
    }
}
