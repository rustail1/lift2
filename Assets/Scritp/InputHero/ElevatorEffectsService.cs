using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ElevatorEffectsService : MonoBehaviour
{
    [Serializable]
    private class ShakeBinding
    {
        public string EventName = "ElevatorStuck";
        public Transform Target;
        public float Duration = 0.5f;
        public float PositionAmount = 0.05f;
        public float RotationAmount = 1.5f;
        public float Frequency = 28f;
    }

    [Serializable]
    private class AnimatorBinding
    {
        public string EventName = "ElevatorFreeFall";
        public Animator Animator;
        public string TriggerName = "Drop";
    }

    [Serializable]
    private class FlickerBinding
    {
        public string EventName = "PowerLost";
        public Light[] Lights;
        public float Duration = 1.5f;
        public float Interval = 0.08f;
        public bool EndEnabled;
    }

    [Header("Shake")]
    [SerializeField] private ShakeBinding[] _shakeBindings;

    [Header("Animator")]
    [SerializeField] private AnimatorBinding[] _animatorBindings;

    [Header("Light Flicker")]
    [SerializeField] private FlickerBinding[] _flickerBindings;

    [Header("Debug Shake")]
    [SerializeField] private bool _isShaking;
    [SerializeField] private string _activeShakeEvent;

    [Header("Debug Flicker")]
    [SerializeField] private bool _isFlickering;
    [SerializeField] private string _activeFlickerEvent;

    private Transform _shakeTarget;
    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _shakeTimer;
    private float _shakeDuration;
    private float _shakePositionAmount;
    private float _shakeRotationAmount;
    private float _shakeFrequency;

    private Light[] _flickerLights;
    private float _flickerTimer;
    private float _flickerDuration;
    private float _flickerInterval;
    private float _nextFlickerTime;
    private bool _flickerEndEnabled;

    public void Initialize()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
        GameEventBus.OnGameEvent += OnGameEvent;
        StopShakeImmediate();
        StopFlickerImmediate();
    }

    public void Tick(float deltaTime)
    {
        TickShake(deltaTime);
        TickFlicker(deltaTime);
    }

    private void OnGameEvent(string eventId)
    {
        HandleShake(eventId);
        HandleAnimator(eventId);
        HandleFlicker(eventId);
    }

    private void HandleShake(string eventId)
    {
        if (_shakeBindings == null)
            return;

        for (int i = 0; i < _shakeBindings.Length; i++)
        {
            ShakeBinding binding = _shakeBindings[i];
            if (binding == null || binding.EventName != eventId || binding.Target == null)
                continue;

            StartShake(binding);
        }
    }

    private void HandleAnimator(string eventId)
    {
        if (_animatorBindings == null)
            return;

        for (int i = 0; i < _animatorBindings.Length; i++)
        {
            AnimatorBinding binding = _animatorBindings[i];
            if (binding == null || binding.EventName != eventId || binding.Animator == null || string.IsNullOrWhiteSpace(binding.TriggerName))
                continue;

            binding.Animator.SetTrigger(binding.TriggerName);
        }
    }

    private void HandleFlicker(string eventId)
    {
        if (_flickerBindings == null)
            return;

        for (int i = 0; i < _flickerBindings.Length; i++)
        {
            FlickerBinding binding = _flickerBindings[i];
            if (binding == null || binding.EventName != eventId)
                continue;

            StartFlicker(binding);
        }
    }

    private void StartShake(ShakeBinding binding)
    {
        StopShakeImmediate();

        _shakeTarget = binding.Target;
        _baseLocalPosition = _shakeTarget.localPosition;
        _baseLocalRotation = _shakeTarget.localRotation;
        _shakeTimer = 0f;
        _shakeDuration = Mathf.Max(0.01f, binding.Duration);
        _shakePositionAmount = binding.PositionAmount;
        _shakeRotationAmount = binding.RotationAmount;
        _shakeFrequency = Mathf.Max(1f, binding.Frequency);
        _activeShakeEvent = binding.EventName;
        _isShaking = true;
    }

    private void TickShake(float deltaTime)
    {
        if (!_isShaking || _shakeTarget == null)
            return;

        _shakeTimer += deltaTime;
        float normalized = Mathf.Clamp01(_shakeTimer / _shakeDuration);
        float power = 1f - normalized;
        float time = Time.time * _shakeFrequency;

        Vector3 offset = new Vector3(
            Mathf.PerlinNoise(time, 0.1f) - 0.5f,
            Mathf.PerlinNoise(0.2f, time) - 0.5f,
            Mathf.PerlinNoise(time, time) - 0.5f
        ) * (_shakePositionAmount * power);

        Vector3 euler = new Vector3(
            (Mathf.PerlinNoise(time, 1.1f) - 0.5f) * _shakeRotationAmount * power,
            (Mathf.PerlinNoise(1.2f, time) - 0.5f) * _shakeRotationAmount * power,
            (Mathf.PerlinNoise(time, 1.3f) - 0.5f) * _shakeRotationAmount * power
        );

        _shakeTarget.localPosition = _baseLocalPosition + offset;
        _shakeTarget.localRotation = _baseLocalRotation * Quaternion.Euler(euler);

        if (_shakeTimer >= _shakeDuration)
            StopShakeImmediate();
    }

    private void StopShakeImmediate()
    {
        if (_shakeTarget != null)
        {
            _shakeTarget.localPosition = _baseLocalPosition;
            _shakeTarget.localRotation = _baseLocalRotation;
        }

        _isShaking = false;
        _activeShakeEvent = string.Empty;
        _shakeTarget = null;
    }

    private void StartFlicker(FlickerBinding binding)
    {
        _flickerLights = binding.Lights;
        _flickerTimer = 0f;
        _flickerDuration = Mathf.Max(0f, binding.Duration);
        _flickerInterval = Mathf.Max(0.01f, binding.Interval);
        _nextFlickerTime = 0f;
        _flickerEndEnabled = binding.EndEnabled;
        _activeFlickerEvent = binding.EventName;
        _isFlickering = true;
    }

    private void TickFlicker(float deltaTime)
    {
        if (!_isFlickering)
            return;

        _flickerTimer += deltaTime;
        _nextFlickerTime -= deltaTime;

        if (_nextFlickerTime <= 0f)
        {
            SetFlickerLights(UnityEngine.Random.value > 0.5f);
            _nextFlickerTime = _flickerInterval;
        }

        if (_flickerTimer >= _flickerDuration)
            StopFlickerImmediate();
    }

    private void SetFlickerLights(bool enabled)
    {
        if (_flickerLights == null)
            return;

        for (int i = 0; i < _flickerLights.Length; i++)
            if (_flickerLights[i] != null)
                _flickerLights[i].enabled = enabled;
    }

    private void StopFlickerImmediate()
    {
        if (_isFlickering)
            SetFlickerLights(_flickerEndEnabled);

        _isFlickering = false;
        _activeFlickerEvent = string.Empty;
        _flickerLights = null;
    }

    private void OnDestroy()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
    }
}
