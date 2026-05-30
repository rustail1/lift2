using UnityEngine;

[DisallowMultipleComponent]
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string _interactionName = "Interact";
    [SerializeField] private InteractionType _interactionType = InteractionType.Click;

    [Header("Conditions")]
    [Tooltip("Какой предмет нужен. Пусто = предмет не нужен. Например: Screwdriver.")]
    [SerializeField] private string _requiredItem;

    [Tooltip("Какое событие должно уже произойти. Пусто = событие не нужно. Например: BagOpened.")]
    [SerializeField] private string _requiredEvent;

    [Header("Result")]
    [Tooltip("Какое событие сработает при успешном взаимодействии. Например: PanelOpened.")]
    [SerializeField] private string _successEvent;

    [Tooltip("Текст, если объект сейчас нельзя использовать.")]
    [SerializeField] private string _failText = "Сейчас это не нужно.";

    [Header("Hold")]
    [SerializeField] private float _holdTime = 1.5f;

    [Header("Mash")]
    [SerializeField] private int _mashClicksRequired = 5;
    [SerializeField] private float _mashResetDelay = 1.0f;

    [Header("Options")]
    [Tooltip("Если включено, объект можно успешно использовать только один раз.")]
    [SerializeField] private bool _oneShot = true;

    [Tooltip("Если включено, объект отключится после успешного взаимодействия.")]
    [SerializeField] private bool _disableAfterSuccess;

    [Header("Debug")]
    [SerializeField] private bool _wasUsed;
    [SerializeField] private bool _isInteractionInProgress;
    [SerializeField] private float _holdProgress;
    [SerializeField] private int _currentMashClicks;

    private float _holdTimer;
    private float _mashTimer;

    public bool IsInteractionInProgress => _isInteractionInProgress;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public virtual void Initialize()
    {
        _holdTimer = 0f;
        _mashTimer = 0f;
        _holdProgress = 0f;
        _currentMashClicks = 0;
        _isInteractionInProgress = false;
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public virtual void InteractionTick(float deltaTime)
    {
        if (_interactionType == InteractionType.Hold && _isInteractionInProgress)
            TickHold(deltaTime);

        if (_interactionType == InteractionType.Mash && _currentMashClicks > 0)
            TickMashReset(deltaTime);
    }

    public virtual string GetInteractionText()
    {
        if (CanInteract())
            return _interactionName;

        return _failText;
    }

    public virtual bool CanInteract()
    {
        if (_oneShot && _wasUsed)
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

    public virtual void InteractDown()
    {
        if (!CanInteract())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            ResetProgress();
            return;
        }

        switch (_interactionType)
        {
            case InteractionType.Click:
                CompleteInteraction();
                break;

            case InteractionType.Hold:
                _isInteractionInProgress = true;
                break;

            case InteractionType.Mash:
                AddMashClick();
                break;
        }
    }

    public virtual void InteractUp()
    {
        if (_interactionType != InteractionType.Hold)
            return;

        _isInteractionInProgress = false;
        _holdTimer = 0f;
        _holdProgress = 0f;
        InteractionUIService.Instance?.SetHoldProgress(0f);
    }

    public virtual InteractionType GetInteractionType()
    {
        return _interactionType;
    }

    public virtual float GetInteractionProgress()
    {
        switch (_interactionType)
        {
            case InteractionType.Hold:
                return _holdProgress;

            case InteractionType.Mash:
                if (_mashClicksRequired <= 0)
                    return 0f;

                return Mathf.Clamp01((float)_currentMashClicks / _mashClicksRequired);

            default:
                return 0f;
        }
    }

    protected virtual void CompleteInteraction()
    {
        _wasUsed = true;
        ResetProgress();

        if (!string.IsNullOrWhiteSpace(_successEvent))
        {
            if (ScenarioStateService.Instance != null)
                ScenarioStateService.Instance.TriggerEvent(_successEvent);
            else
                GameEventBus.Trigger(_successEvent);
        }

        if (_disableAfterSuccess)
            gameObject.SetActive(false);
    }

    protected void ResetProgress()
    {
        _isInteractionInProgress = false;
        _holdTimer = 0f;
        _mashTimer = 0f;
        _holdProgress = 0f;
        _currentMashClicks = 0;

        InteractionUIService.Instance?.SetHoldProgress(0f);
        InteractionUIService.Instance?.SetMashProgress(0f);
    }

    private void TickHold(float deltaTime)
    {
        if (!CanInteract())
        {
            ResetProgress();
            return;
        }

        _holdTimer += deltaTime;
        _holdProgress = _holdTime <= 0f ? 1f : Mathf.Clamp01(_holdTimer / _holdTime);
        InteractionUIService.Instance?.SetHoldProgress(_holdProgress);

        if (_holdProgress >= 1f)
            CompleteInteraction();
    }

    private void AddMashClick()
    {
        _currentMashClicks++;
        _mashTimer = _mashResetDelay;
        InteractionUIService.Instance?.SetMashProgress(GetInteractionProgress());

        if (_currentMashClicks >= _mashClicksRequired)
            CompleteInteraction();
    }

    private void TickMashReset(float deltaTime)
    {
        _mashTimer -= deltaTime;

        if (_mashTimer > 0f)
            return;

        _currentMashClicks = 0;
        _mashTimer = 0f;
        InteractionUIService.Instance?.SetMashProgress(0f);
    }
}
