using UnityEngine;

[DisallowMultipleComponent]
public class TransitionPoint : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string _interactionName = "Перейти";

    [Header("Target")]
    [SerializeField] private Transform _playerRoot;
    [SerializeField] private Transform _targetSpawn;

    [Header("Conditions")]
    [SerializeField] private string _requiredItem;
    [SerializeField] private string _requiredEvent;
    [SerializeField] private string _failText = "Сейчас сюда нельзя.";

    [Header("Fade")]
    [SerializeField] private bool _useScreenFade = true;
    [SerializeField] private float _fadeOutTime = 0.35f;
    [SerializeField] private float _holdBlackTime = 0.15f;
    [SerializeField] private float _fadeInTime = 0.35f;

    [Header("Output Event")]
    [SerializeField] private string _successEvent = "TransitionCompleted";
    [SerializeField] private bool _saveSuccessToScenarioState = true;

    [Header("Options")]
    [SerializeField] private bool _oneShot;
    [SerializeField] private bool _disableAfterSuccess;

    [Header("Debug")]
    [SerializeField] private bool _wasUsed;
    [SerializeField] private bool _isTransitioning;

    public bool IsInteractionInProgress => _isTransitioning;

    public void Initialize()
    {
        _isTransitioning = false;

        if (_playerRoot == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerRoot = player.transform;
        }
    }

    public string GetInteractionText()
    {
        return CanInteract() ? _interactionName : _failText;
    }

    public bool CanInteract()
    {
        if (_isTransitioning)
            return false;

        if (_oneShot && _wasUsed)
            return false;

        if (_targetSpawn == null)
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

        _isTransitioning = true;
        _wasUsed = true;
        PlayerControlLockService.Instance?.LockPlayer("Transition");

        if (_useScreenFade && ScreenFadeService.Instance != null)
        {
            ScreenFadeService.Instance.BeginTransition(DoTeleportAndFinish, _fadeOutTime, _holdBlackTime, _fadeInTime);
        }
        else
        {
            DoTeleportAndFinish();
            PlayerControlLockService.Instance?.UnlockPlayer("Transition");
        }
    }

    public void InteractUp() { }
    public void InteractionTick(float deltaTime) { }
    public float GetInteractionProgress() => 0f;
    public InteractionType GetInteractionType() => InteractionType.Click;

    private void DoTeleportAndFinish()
    {
        TeleportPlayer();
        FireSuccessEvent();
        _isTransitioning = false;

        // Если есть fade, игрок разблокируется сразу после телепорта.
        // При необходимости можно держать блокировку дольше отдельным PlayerControlLock событием.
        PlayerControlLockService.Instance?.UnlockPlayer("Transition");

        if (_disableAfterSuccess)
            gameObject.SetActive(false);
    }

    private void TeleportPlayer()
    {
        if (_targetSpawn == null)
            return;

        if (_playerRoot == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerRoot = player.transform;
        }

        if (_playerRoot == null)
            return;

        CharacterController controller = _playerRoot.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        _playerRoot.position = _targetSpawn.position;
        _playerRoot.rotation = _targetSpawn.rotation;

        if (controller != null)
            controller.enabled = true;
    }

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
