using UnityEngine;

[DisallowMultipleComponent]
public class AnimatorTriggerOnEvent : MonoBehaviour
{
    [Header("Event")]
    [Tooltip("Событие, после которого нужно вызвать Animator trigger. Например: HatchOpened.")]
    [SerializeField] private string _eventName = "HatchOpened";

    [Header("Animator")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _triggerName = "Open";

    [Header("Options")]
    [Tooltip("Если включено, реакция сработает только один раз.")]
    [SerializeField] private bool _oneShot = true;

    [Header("Debug")]
    [SerializeField] private bool _isListening;
    [SerializeField] private bool _wasTriggered;

    private void Reset()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();

        if (_isListening)
            GameEventBus.OnGameEvent -= OnGameEvent;

        GameEventBus.OnGameEvent += OnGameEvent;
        _isListening = true;
    }

    private void OnDestroy()
    {
        if (!_isListening)
            return;

        GameEventBus.OnGameEvent -= OnGameEvent;
        _isListening = false;
    }

    private void OnGameEvent(string eventId)
    {
        if (eventId != _eventName)
            return;

        if (_oneShot && _wasTriggered)
            return;

        _wasTriggered = true;
        TriggerAnimator();
    }

    private void TriggerAnimator()
    {
        if (_animator == null || string.IsNullOrWhiteSpace(_triggerName))
            return;

        _animator.SetTrigger(_triggerName);
    }
}
