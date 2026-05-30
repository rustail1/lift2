using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InspectableZone : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string _interactionName = "Осмотреть";

    [Header("View")]
    [Tooltip("Точка, куда камера должна приблизиться. Поставь пустой GameObject перед панелью и разверни его в сторону панели.")]
    [SerializeField] private Transform _viewPoint;
    [SerializeField] private float _enterSpeed = 8f;
    [SerializeField] private float _exitSpeed = 10f;

    [Header("Conditions")]
    [SerializeField] private string _requiredItem;
    [SerializeField] private string _requiredEvent;
    [SerializeField] private string _failText = "Сейчас это не нужно.";

    [Header("Events")]
    [SerializeField] private string _enterEvent;
    [SerializeField] private string _exitEvent;
    [SerializeField] private bool _saveEventsToScenarioState = true;

    [Header("Options")]
    [Tooltip("Если включено, коллайдеры этой зоны выключаются на время inspect mode, чтобы не перекрывать кнопки/тумблеры внутри панели.")]
    [SerializeField] private bool _disableOwnCollidersWhileInspecting = true;

    [Tooltip("Если включено, интерактивы внутри панели будут выключены до входа в inspect mode и включатся только в крупном плане.")]
    [SerializeField] private bool _childInteractablesOnlyInInspection = true;

    [Tooltip("Автоматически искать дочерние интерактивы: KeypadButton, InteractableObject и другие IInteractable.")]
    [SerializeField] private bool _autoCollectChildInteractables = true;

    [Tooltip("Дополнительные коллайдеры кнопок/тумблеров, которые нужно включать только в inspect mode. Заполняй вручную, если авто-поиск не нашёл нужные кнопки.")]
    [SerializeField] private Collider[] _inspectionOnlyColliders;

    [SerializeField] private bool _oneShot;

    [Header("Debug")]
    [SerializeField] private bool _isInspecting;
    [SerializeField] private bool _wasUsed;
    [SerializeField] private int _managedChildColliderCount;

    private Collider[] _ownColliders;
    private Collider[] _managedChildColliders;

    public bool IsInteractionInProgress => false;

    public void Initialize()
    {
        _ownColliders = GetComponents<Collider>();

        if (_autoCollectChildInteractables)
            CollectChildInteractableColliders();
        else
            _managedChildColliders = _inspectionOnlyColliders;

        _isInspecting = false;

        if (_childInteractablesOnlyInInspection)
            SetChildInteractableCollidersEnabled(false);
    }

    [ContextMenu("Collect Child Interactable Colliders")]
    public void CollectChildInteractableColliders()
    {
        HashSet<Collider> own = new HashSet<Collider>();
        _ownColliders = GetComponents<Collider>();

        for (int i = 0; i < _ownColliders.Length; i++)
            if (_ownColliders[i] != null)
                own.Add(_ownColliders[i]);

        List<Collider> result = new List<Collider>();
        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour == this)
                continue;

            if (!(behaviour is IInteractable))
                continue;

            Collider[] colliders = behaviour.GetComponents<Collider>();
            for (int c = 0; c < colliders.Length; c++)
            {
                Collider collider = colliders[c];
                if (collider == null || own.Contains(collider) || result.Contains(collider))
                    continue;

                result.Add(collider);
            }

            Collider[] childColliders = behaviour.GetComponentsInChildren<Collider>(true);
            for (int c = 0; c < childColliders.Length; c++)
            {
                Collider collider = childColliders[c];
                if (collider == null || own.Contains(collider) || result.Contains(collider))
                    continue;

                result.Add(collider);
            }
        }

        if (_inspectionOnlyColliders != null)
        {
            for (int i = 0; i < _inspectionOnlyColliders.Length; i++)
            {
                Collider collider = _inspectionOnlyColliders[i];
                if (collider != null && !own.Contains(collider) && !result.Contains(collider))
                    result.Add(collider);
            }
        }

        _managedChildColliders = result.ToArray();
        _managedChildColliderCount = _managedChildColliders.Length;
    }

    public string GetInteractionText()
    {
        return CanInteract() ? _interactionName : _failText;
    }

    public bool CanInteract()
    {
        if (_oneShot && _wasUsed)
            return false;

        if (_viewPoint == null)
            return false;

        if (InspectionViewService.Instance != null && InspectionViewService.Instance.IsInspecting)
            return false;

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

    public void InteractDown()
    {
        if (!CanInteract())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            return;
        }

        if (InspectionViewService.Instance == null)
        {
            InteractionUIService.Instance?.ShowHint("InspectionViewService не найден.");
            return;
        }

        bool started = InspectionViewService.Instance.EnterInspection(this, _viewPoint, _enterSpeed, _exitSpeed);
        if (!started)
        {
            InteractionUIService.Instance?.ShowHint("Не удалось открыть крупный план.");
            return;
        }

        _wasUsed = true;
        FireEvent(_enterEvent);
    }

    public void InteractUp() { }
    public void InteractionTick(float deltaTime) { }
    public float GetInteractionProgress() => 0f;
    public InteractionType GetInteractionType() => InteractionType.Click;

    public void OnInspectionEnteredInternal()
    {
        _isInspecting = true;

        if (_disableOwnCollidersWhileInspecting)
            SetOwnCollidersEnabled(false);

        if (_childInteractablesOnlyInInspection)
            SetChildInteractableCollidersEnabled(true);
    }

    public void OnInspectionExitedInternal()
    {
        _isInspecting = false;
        SetOwnCollidersEnabled(true);

        if (_childInteractablesOnlyInInspection)
            SetChildInteractableCollidersEnabled(false);

        FireEvent(_exitEvent);
    }

    private void SetOwnCollidersEnabled(bool enabled)
    {
        if (_ownColliders == null)
            _ownColliders = GetComponents<Collider>();

        for (int i = 0; i < _ownColliders.Length; i++)
            if (_ownColliders[i] != null)
                _ownColliders[i].enabled = enabled;
    }

    private void SetChildInteractableCollidersEnabled(bool enabled)
    {
        if (_managedChildColliders == null)
        {
            if (_autoCollectChildInteractables)
                CollectChildInteractableColliders();
            else
                _managedChildColliders = _inspectionOnlyColliders;
        }

        if (_managedChildColliders == null)
            return;

        for (int i = 0; i < _managedChildColliders.Length; i++)
            if (_managedChildColliders[i] != null)
                _managedChildColliders[i].enabled = enabled;
    }

    private void FireEvent(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        if (_saveEventsToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(eventName);
        else
            GameEventBus.Trigger(eventName);
    }
}
