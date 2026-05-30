using UnityEngine;

[DisallowMultipleComponent]
public class TriggerEventFromAnimation : MonoBehaviour
{
    [Header("Configured Event")]
    [SerializeField] private string _eventName = "AnimationFinished";
    [SerializeField] private bool _saveToScenarioState = true;

    [Header("Debug")]
    [SerializeField] private string _lastFiredEvent;

    public void FireConfiguredEvent()
    {
        Fire(_eventName);
    }

    // Можно вызвать Animation Event-ом и передать имя события строкой.
    public void FireEvent(string eventName)
    {
        Fire(string.IsNullOrWhiteSpace(eventName) ? _eventName : eventName);
    }

    private void Fire(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        _lastFiredEvent = eventName;

        if (_saveToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(eventName);
        else
            GameEventBus.Trigger(eventName);
    }
}
