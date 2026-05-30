using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ScreenFadeService : MonoBehaviour
{
    public static ScreenFadeService Instance { get; private set; }

    private enum FadeState
    {
        Idle,
        FadingOut,
        HoldingBlack,
        FadingIn
    }

    [Header("UI")]
    [SerializeField] private CanvasGroup _fadeCanvasGroup;

    [Header("Default Settings")]
    [SerializeField] private float _defaultFadeOutTime = 0.35f;
    [SerializeField] private float _defaultHoldTime = 0.15f;
    [SerializeField] private float _defaultFadeInTime = 0.35f;

    [Header("Options")]
    [SerializeField] private bool _blockRaycastsWhileFading = true;

    [Header("Debug")]
    [SerializeField] private FadeState _state;
    [SerializeField] private bool _isFading;
    [SerializeField] private float _alpha;

    private Action _onBlack;
    private float _timer;
    private float _fadeOutTime;
    private float _holdTime;
    private float _fadeInTime;

    public bool IsFading => _isFading;

    public void Initialize()
    {
        Instance = this;
        _state = FadeState.Idle;
        _isFading = false;
        _onBlack = null;
        SetAlpha(0f);
    }

    public void Tick(float deltaTime)
    {
        switch (_state)
        {
            case FadeState.FadingOut:
                TickFadeOut(deltaTime);
                break;

            case FadeState.HoldingBlack:
                TickHold(deltaTime);
                break;

            case FadeState.FadingIn:
                TickFadeIn(deltaTime);
                break;
        }
    }

    public void BeginTransition(Action onBlack)
    {
        BeginTransition(onBlack, _defaultFadeOutTime, _defaultHoldTime, _defaultFadeInTime);
    }

    public void BeginTransition(Action onBlack, float fadeOutTime, float holdTime, float fadeInTime)
    {
        _onBlack = onBlack;
        _fadeOutTime = Mathf.Max(0.001f, fadeOutTime);
        _holdTime = Mathf.Max(0f, holdTime);
        _fadeInTime = Mathf.Max(0.001f, fadeInTime);
        _timer = 0f;
        _state = FadeState.FadingOut;
        _isFading = true;
        ApplyCanvasInteraction(true);
    }

    private void TickFadeOut(float deltaTime)
    {
        _timer += deltaTime;
        float t = Mathf.Clamp01(_timer / _fadeOutTime);
        SetAlpha(t);

        if (t < 1f)
            return;

        _onBlack?.Invoke();
        _onBlack = null;
        _timer = 0f;
        _state = FadeState.HoldingBlack;
    }

    private void TickHold(float deltaTime)
    {
        _timer += deltaTime;
        SetAlpha(1f);

        if (_timer < _holdTime)
            return;

        _timer = 0f;
        _state = FadeState.FadingIn;
    }

    private void TickFadeIn(float deltaTime)
    {
        _timer += deltaTime;
        float t = Mathf.Clamp01(_timer / _fadeInTime);
        SetAlpha(1f - t);

        if (t < 1f)
            return;

        _timer = 0f;
        _state = FadeState.Idle;
        _isFading = false;
        ApplyCanvasInteraction(false);
    }

    private void SetAlpha(float alpha)
    {
        _alpha = Mathf.Clamp01(alpha);
        if (_fadeCanvasGroup != null)
            _fadeCanvasGroup.alpha = _alpha;
    }

    private void ApplyCanvasInteraction(bool active)
    {
        if (_fadeCanvasGroup == null)
            return;

        _fadeCanvasGroup.blocksRaycasts = _blockRaycastsWhileFading && active;
        _fadeCanvasGroup.interactable = _blockRaycastsWhileFading && active;
    }
}
