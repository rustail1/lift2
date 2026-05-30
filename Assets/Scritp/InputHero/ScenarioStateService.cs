using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ScenarioStateService : MonoBehaviour
{
    public static ScenarioStateService Instance { get; private set; }

    [Header("Start Events")]
    [Tooltip("События, которые уже считаются выполненными при старте сцены. Например: GameStarted.")]
    [SerializeField] private string[] _startEvents;

    [Header("Debug")]
    [Tooltip("Только для просмотра в Inspector. Unity не показывает HashSet, поэтому держим копию списком.")]
    [SerializeField] private List<string> _completedEventsDebug = new List<string>();

    private readonly HashSet<string> _completedEvents = new HashSet<string>();

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        Instance = this;
        _completedEvents.Clear();

        if (_startEvents != null)
        {
            for (int i = 0; i < _startEvents.Length; i++)
                AddEventSilently(_startEvents[i]);
        }

        SyncDebugList();
    }

    public bool HasEvent(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return true;

        return _completedEvents.Contains(eventId);
    }

    public void TriggerEvent(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        if (_completedEvents.Add(eventId))
            SyncDebugList();

        GameEventBus.Trigger(eventId);
    }

    public void ResetEvents()
    {
        _completedEvents.Clear();
        SyncDebugList();
    }

    private void AddEventSilently(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        _completedEvents.Add(eventId);
    }

    private void SyncDebugList()
    {
        _completedEventsDebug.Clear();
        foreach (string eventId in _completedEvents)
            _completedEventsDebug.Add(eventId);
    }
}
