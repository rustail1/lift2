using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class EmergencyTimerService : MonoBehaviour
{
    public static EmergencyTimerService Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private float _totalSeconds = 600f;
    [SerializeField] private bool _startActive;

    [Header("Input Events")]
    [SerializeField] private string _startEvent = "TimerStarted";
    [SerializeField] private string _pauseEvent = "TimerPaused";
    [SerializeField] private string _resumeEvent = "TimerResumed";
    [SerializeField] private string _stopEvent = "TimerStopped";

    [Header("Output Events")]
    [SerializeField] private string _lowTimeEvent = "TimerLow";
    [SerializeField] private float _lowTimeThreshold = 60f;
    [SerializeField] private string _endedEvent = "TimerEnded";
    [SerializeField] private bool _saveOutputEventsToScenarioState = true;

    [Header("UI")]
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private bool _hideWhenInactive;

    [Header("Debug")]
    [SerializeField] private bool _isActive;
    [SerializeField] private bool _isPaused;
    [SerializeField] private bool _lowTimeFired;
    [SerializeField] private float _currentSeconds;

    public bool IsActive => _isActive;
    public bool IsPaused => _isPaused;
    public float CurrentSeconds => _currentSeconds;

    public void Initialize()
    {
        Instance = this;
        GameEventBus.OnGameEvent -= OnGameEvent;
        GameEventBus.OnGameEvent += OnGameEvent;

        _currentSeconds = Mathf.Max(0f, _totalSeconds);
        _isActive = _startActive;
        _isPaused = false;
        _lowTimeFired = false;
        UpdateUI();
    }

    public void Tick(float deltaTime)
    {
        if (!_isActive || _isPaused)
        {
            UpdateUI();
            return;
        }

        _currentSeconds -= deltaTime;

        if (!_lowTimeFired && _currentSeconds <= _lowTimeThreshold)
        {
            _lowTimeFired = true;
            FireEvent(_lowTimeEvent);
        }

        if (_currentSeconds <= 0f)
        {
            _currentSeconds = 0f;
            _isActive = false;
            FireEvent(_endedEvent);
        }

        UpdateUI();
    }

    public void StartTimer()
    {
        _currentSeconds = Mathf.Max(0f, _totalSeconds);
        _isActive = true;
        _isPaused = false;
        _lowTimeFired = false;
        UpdateUI();
    }

    public void PauseTimer()
    {
        if (_isActive)
            _isPaused = true;
    }

    public void ResumeTimer()
    {
        if (_isActive)
            _isPaused = false;
    }

    public void StopTimer()
    {
        _isActive = false;
        _isPaused = false;
        UpdateUI();
    }

    private void OnGameEvent(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(_startEvent) && eventId == _startEvent)
            StartTimer();

        if (!string.IsNullOrWhiteSpace(_pauseEvent) && eventId == _pauseEvent)
            PauseTimer();

        if (!string.IsNullOrWhiteSpace(_resumeEvent) && eventId == _resumeEvent)
            ResumeTimer();

        if (!string.IsNullOrWhiteSpace(_stopEvent) && eventId == _stopEvent)
            StopTimer();
    }

    private void UpdateUI()
    {
        if (_timerText == null)
            return;

        if (_hideWhenInactive && !_isActive)
        {
            _timerText.gameObject.SetActive(false);
            return;
        }

        _timerText.gameObject.SetActive(true);

        int total = Mathf.CeilToInt(_currentSeconds);
        int minutes = total / 60;
        int seconds = total % 60;
        _timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    private void FireEvent(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        if (_saveOutputEventsToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(eventName);
        else
            GameEventBus.Trigger(eventName);
    }

    private void OnDestroy()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
    }
}
