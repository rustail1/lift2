using UnityEngine;

[DisallowMultipleComponent]
public class DisableOnEvent : MonoBehaviour
{
    [Header("Event")]
    [Tooltip("Событие, после которого объект/компонент будет выключен. Например: BagOpened.")]
    [SerializeField] private string _eventName = "BagOpened";

    [Header("Targets")]
    [Tooltip("GameObject-ы, которые нужно выключить через SetActive(false).")]
    [SerializeField] private GameObject[] _targets;

    [Tooltip("Behaviour-компоненты, которые нужно выключить через enabled = false. Например InteractableObject, Light, MonoBehaviour.")]
    [SerializeField] private Behaviour[] _behaviours;

    [Tooltip("Collider-ы, которые нужно выключить через enabled = false.")]
    [SerializeField] private Collider[] _colliders;

    [Header("Options")]
    [Tooltip("Если включено, реакция сработает только один раз.")]
    [SerializeField] private bool _oneShot = true;

    [Header("Debug")]
    [SerializeField] private bool _isListening;
    [SerializeField] private bool _wasTriggered;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
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
        ApplyDisable();
    }

    private void ApplyDisable()
    {
        if (_targets != null)
        {
            for (int i = 0; i < _targets.Length; i++)
                if (_targets[i] != null)
                    _targets[i].SetActive(false);
        }

        if (_behaviours != null)
        {
            for (int i = 0; i < _behaviours.Length; i++)
                if (_behaviours[i] != null)
                    _behaviours[i].enabled = false;
        }

        if (_colliders != null)
        {
            for (int i = 0; i < _colliders.Length; i++)
                if (_colliders[i] != null)
                    _colliders[i].enabled = false;
        }
    }
}
