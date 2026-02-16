using System;
using System.Collections;
using UnityEngine;

public sealed class RoomTransitionController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private float fadeInTime = 0.25f;

    [SerializeField] private GameObject playerVisualRoot;

    public event Action OnTransitionStart;
    public event Action OnTransitionEnd;

    public IEnumerator RunTransition(Action swapAction)
    {
        OnTransitionStart?.Invoke();

        yield return FadeTo(1f, fadeOutTime);

        if (playerVisualRoot != null) playerVisualRoot.SetActive(false);

        swapAction?.Invoke();

        if (playerVisualRoot != null) playerVisualRoot.SetActive(true);

        yield return FadeTo(0f, fadeInTime);

        OnTransitionEnd?.Invoke();
    }

    private IEnumerator FadeTo(float target, float time)
    {
        if (fade == null) yield break;

        float start = fade.alpha;

        if (time <= 0f)
        {
            fade.alpha = target;
            yield break;
        }

        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / time);
            fade.alpha = Mathf.Lerp(start, target, u);
            yield return null;
        }

        fade.alpha = target;
    }
}