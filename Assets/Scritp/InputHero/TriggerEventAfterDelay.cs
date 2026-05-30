using UnityEngine;

[DisallowMultipleComponent]
public class TriggerEventAfterDelay : MonoBehaviour
{
    [Header("Input Event")]
    [Tooltip("Событие, после которого стартует задержка. Например: HatchOpened.")]
    [SerializeField] private string _listenEvent = "HatchOpened";

    [Header("Output Event")]
    [Tooltip("Событие, которое будет вызвано после задержки. Например: Intercom_HatchOpened.")]
    [SerializeField] private string _triggerEvent = "Intercom_HatchOpened";
    [SerializeField] private float _delay = 1.5f;

    [Header("Options")]
    [Tooltip("Если включено, реакция сработает только один раз.")]
    [SerializeField] private bool _oneShot = true;

    [Tooltip("Если включено, событие будет записано в ScenarioStateService. Если выключено — только уйдёт в GameEventBus.")]
    [SerializeField] private bool _saveToScenarioState = true;

    [Header("Debug")]
    [SerializeField] private bool _isListening;
    [SerializeField] private bool _wasTriggered;
    [SerializeField] private bool _isWaiting;
    [SerializeField] private float _timer;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        _isWaiting = false;
        _timer = 0f;

        if (_isListening)
            GameEventBus.OnGameEvent -= OnGameEvent;

        GameEventBus.OnGameEvent += OnGameEvent;
        _isListening = true;
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick(float deltaTime)
    {
        if (!_isWaiting)
            return;

        _timer -= deltaTime;
        if (_timer > 0f)
            return;

        _isWaiting = false;
        FireDelayedEvent();
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
        if (eventId != _listenEvent)
            return;

        if (_oneShot && _wasTriggered)
            return;

        _wasTriggered = true;

        if (_delay <= 0f)
        {
            FireDelayedEvent();
            return;
        }

        _timer = _delay;
        _isWaiting = true;
    }

    private void FireDelayedEvent()
    {
        if (string.IsNullOrWhiteSpace(_triggerEvent))
            return;

        if (_saveToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(_triggerEvent);
        else
            GameEventBus.Trigger(_triggerEvent);
    }
}
