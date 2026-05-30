using UnityEngine;

[DisallowMultipleComponent]
public class DocumentItem : MonoBehaviour, IInteractable
{
    [Header("Document")]
    [SerializeField] private string _interactionName = "Читать документ";
    [SerializeField] private DocumentData _document;

    [Header("Conditions")]
    [SerializeField] private string _requiredItem;
    [SerializeField] private string _requiredEvent;
    [SerializeField] private string _failText = "Сейчас это не нужно.";

    [Header("Events")]
    [SerializeField] private string _successEvent;
    [SerializeField] private bool _saveSuccessToScenarioState = true;

    [Header("Options")]
    [SerializeField] private bool _oneShot;
    [SerializeField] private bool _disableAfterOpen;

    [Header("Debug")]
    [SerializeField] private bool _wasOpened;

    public bool IsInteractionInProgress => false;

    public void Initialize()
    {
        _wasOpened = false;
    }

    public string GetInteractionText()
    {
        return CanInteract() ? _interactionName : _failText;
    }

    public bool CanInteract()
    {
        if (_oneShot && _wasOpened)
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

        return _document != null;
    }

    public void InteractDown()
    {
        if (!CanInteract())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            return;
        }

        _wasOpened = true;
        DocumentViewerService.Instance?.OpenDocument(_document);
        FireSuccessEvent();

        if (_disableAfterOpen)
            gameObject.SetActive(false);
    }

    public void InteractUp() { }
    public void InteractionTick(float deltaTime) { }
    public float GetInteractionProgress() => 0f;
    public InteractionType GetInteractionType() => InteractionType.Click;

    private void FireSuccessEvent()
    {
        if (string.IsNullOrWhiteSpace(_successEvent))
            return;

        if (_saveSuccessToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(_successEvent);
        else
            GameEventBus.Trigger(_successEvent);
    }
}
