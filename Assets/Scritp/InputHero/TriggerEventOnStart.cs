using System;
using UnityEngine;

[DisallowMultipleComponent]
public class TriggerEventOnStart : MonoBehaviour
{
    [Serializable]
    private class StartEventEntry
    {
        [Tooltip("Событие, которое нужно вызвать после старта сцены. Например: GameStarted.")]
        public string EventName = "GameStarted";

        [Tooltip("Задержка перед вызовом события. 0 = вызвать на первом Tick после инициализации.")]
        public float Delay;

        [Tooltip("Если включено, событие будет записано в ScenarioStateService. Если выключено — только уйдёт в GameEventBus.")]
        public bool SaveToScenarioState = true;

        [Header("Debug")]
        public bool Fired;
        public float Timer;
    }

    [Header("Start Events")]
    [Tooltip("Список событий, которые будут вызваны после старта сцены.")]
    [SerializeField] private StartEventEntry[] _events;

    [Header("Options")]
    [Tooltip("Если включено, весь список сработает только один раз.")]
    [SerializeField] private bool _oneShot = true;

    [Header("Debug")]
    [SerializeField] private bool _isActive;
    [SerializeField] private bool _wasTriggered;
    [SerializeField] private int _firedCount;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        _isActive = true;
        _wasTriggered = false;
        _firedCount = 0;

        if (_events == null || _events.Length == 0)
        {
            _events = new StartEventEntry[1];
            _events[0] = new StartEventEntry
            {
                EventName = "GameStarted",
                Delay = 0f,
                SaveToScenarioState = true
            };
        }

        for (int i = 0; i < _events.Length; i++)
        {
            if (_events[i] == null)
                continue;

            _events[i].Fired = false;
            _events[i].Timer = Mathf.Max(0f, _events[i].Delay);
        }
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick(float deltaTime)
    {
        if (!_isActive)
            return;

        if (_oneShot && _wasTriggered)
            return;

        bool allFired = true;

        for (int i = 0; i < _events.Length; i++)
        {
            StartEventEntry entry = _events[i];
            if (entry == null || entry.Fired)
                continue;

            allFired = false;
            entry.Timer -= deltaTime;

            if (entry.Timer > 0f)
                continue;

            Fire(entry);
        }

        if (allFired || AllEntriesFired())
        {
            _wasTriggered = true;
            if (_oneShot)
                _isActive = false;
        }
    }

    private bool AllEntriesFired()
    {
        if (_events == null)
            return true;

        for (int i = 0; i < _events.Length; i++)
        {
            if (_events[i] != null && !_events[i].Fired)
                return false;
        }

        return true;
    }

    private void Fire(StartEventEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.EventName))
            return;

        entry.Fired = true;
        _firedCount++;

        if (entry.SaveToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(entry.EventName);
        else
            GameEventBus.Trigger(entry.EventName);
    }
}
