using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class TriggerEventOnExitZone : MonoBehaviour
{
    [Header("Output Event")]
    [Tooltip("Событие, которое будет вызвано, когда объект выйдет из trigger-зоны. Например: CabinExited.")]
    [SerializeField] private string _eventName = "ZoneExited";

    [Header("Filter")]
    [Tooltip("Если заполнено, событие сработает только от объекта с этим Tag. Обычно: Player. Пусто = любой объект.")]
    [SerializeField] private string _requiredTag;

    [Tooltip("Если список заполнен, событие сработает только от этих Collider-ов.")]
    [SerializeField] private Collider[] _allowedColliders;

    [Header("Options")]
    [SerializeField] private bool _oneShot = true;

    [Tooltip("Если включено, событие будет записано в ScenarioStateService. Если выключено — только уйдёт в GameEventBus.")]
    [SerializeField] private bool _saveToScenarioState = true;

    [Header("Debug")]
    [SerializeField] private bool _isInitialized;
    [SerializeField] private bool _wasTriggered;
    [SerializeField] private string _lastExitedObject;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        _isInitialized = true;

        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
            ownCollider.isTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_isInitialized)
            return;

        if (_oneShot && _wasTriggered)
            return;

        if (!IsAllowed(other))
            return;

        _lastExitedObject = other != null ? other.name : string.Empty;
        _wasTriggered = true;
        FireEvent();
    }

    private bool IsAllowed(Collider other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrWhiteSpace(_requiredTag) && !other.CompareTag(_requiredTag))
            return false;

        if (_allowedColliders != null && _allowedColliders.Length > 0)
        {
            for (int i = 0; i < _allowedColliders.Length; i++)
            {
                if (_allowedColliders[i] == other)
                    return true;
            }

            return false;
        }

        return true;
    }

    private void FireEvent()
    {
        if (string.IsNullOrWhiteSpace(_eventName))
            return;

        if (_saveToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(_eventName);
        else
            GameEventBus.Trigger(_eventName);
    }
}
