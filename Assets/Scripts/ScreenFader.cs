using System;
using UnityEngine;

public sealed class ScreenFader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeOutTime = 0.12f;
    [SerializeField] private float fadeInTime = 0.12f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Behavior")]
    [SerializeField] private bool blockRaycastsWhileFading = true;

    private float _t;
    private bool _fadingOut;
    private bool _fadingIn;
    private Action _midAction;
    private bool _midActionFired;

    private void Reset()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponentInChildren<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (_fadingOut) TickFadeOut();
        if (_fadingIn) TickFadeIn();
    }

    public void FadeOutIn(Action midAction)
    {
        if (canvasGroup == null)
        {
            midAction?.Invoke();
            return;
        }

        _midAction = midAction;
        _midActionFired = false;

        _t = 0f;
        _fadingOut = true;
        _fadingIn = false;

        if (blockRaycastsWhileFading) canvasGroup.blocksRaycasts = true;
    }

    private void TickFadeOut()
    {
        _t += Time.deltaTime / Mathf.Max(0.0001f, fadeOutTime);
        float t01 = Mathf.Clamp01(_t);
        float eased = ease != null ? ease.Evaluate(t01) : t01;

        canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);

        if (t01 >= 1f)
        {
            _fadingOut = false;

            if (!_midActionFired)
            {
                _midActionFired = true;
                _midAction?.Invoke();
            }

            _t = 0f;
            _fadingIn = true;
        }
    }

    private void TickFadeIn()
    {
        _t += Time.deltaTime / Mathf.Max(0.0001f, fadeInTime);
        float t01 = Mathf.Clamp01(_t);
        float eased = ease != null ? ease.Evaluate(t01) : t01;

        canvasGroup.alpha = Mathf.Lerp(1f, 0f, eased);

        if (t01 >= 1f)
        {
            _fadingIn = false;
            canvasGroup.alpha = 0f;
            if (blockRaycastsWhileFading) canvasGroup.blocksRaycasts = false;
        }
    }
}
