using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class TriggerEventOnEnterZone : MonoBehaviour
{
    [Header("Output Event")]
    [Tooltip("Событие, которое будет вызвано, когда объект войдёт в trigger-зону. Например: RoofEntered.")]
    [SerializeField] private string _eventName = "ZoneEntered";

    [Header("Conditions")]
    [Tooltip("Предмет, который должен быть у игрока. Пусто = предмет не нужен.")]
    [SerializeField] private string _requiredItem;

    [Tooltip("Событие, которое должно уже произойти. Пусто = событие не нужно.")]
    [SerializeField] private string _requiredEvent;

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
    [SerializeField] private string _lastEnteredObject;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        _isInitialized = true;

        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
            ownCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized)
            return;

        if (_oneShot && _wasTriggered)
            return;

        if (!IsAllowed(other))
            return;

        if (!CheckConditions())
            return;

        _lastEnteredObject = other != null ? other.name : string.Empty;
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

    private bool CheckConditions()
    {
        if (!string.IsNullOrWhiteSpace(_requiredItem))
        {
            InventoryService inventory = InventoryService.Instance;
            if (inventory == null || !inventory.HasItem(_requiredItem))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(_requiredEvent))
        {
            ScenarioStateService scenario = ScenarioStateService.Instance;
            if (scenario == null || !scenario.HasEvent(_requiredEvent))
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
