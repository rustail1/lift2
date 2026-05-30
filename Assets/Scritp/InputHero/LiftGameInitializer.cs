using UnityEngine;

[DisallowMultipleComponent]
public class LiftGameInitializer : MonoBehaviour
{
    [Header("Auto")]
    [SerializeField] private bool _collectOnStart = true;
    [SerializeField] private bool _initializeOnStart = true;

    [Header("Core")]
    [SerializeField] private ScenarioStateService[] _scenarioStates;
    [SerializeField] private InventoryService[] _inventories;
    [SerializeField] private InteractionUIService[] _interactionUis;
    [SerializeField] private SubtitleUIService[] _subtitleUis;
    [SerializeField] private DialogueService[] _dialogueServices;

    [Header("Player Components")]
    [SerializeField] private FpsCharacter[] _characters;
    [SerializeField] private PlayerLook[] _playerLooks;
    [SerializeField] private CameraZoomController[] _cameraZooms;
    [SerializeField] private PlayerInteractor[] _playerInteractors;
    [SerializeField] private FlashlightController[] _flashlights;
    [SerializeField] private CameraMotionController[] _cameraMotions;
    [SerializeField] private FlashlightSwayController[] _flashlightSways;

    [Header("World Components")]
    [SerializeField] private InteractableObject[] _interactables;

    [Header("Event Reactions")]
    [SerializeField] private EnableOnEvent[] _enableOnEvents;
    [SerializeField] private DisableOnEvent[] _disableOnEvents;
    [SerializeField] private AnimatorTriggerOnEvent[] _animatorTriggerOnEvents;
    [SerializeField] private PlaySoundOnEvent[] _playSoundOnEvents;
    [SerializeField] private TriggerEventAfterDelay[] _triggerEventAfterDelays;
    [SerializeField] private TriggerEventOnStart[] _triggerEventOnStarts;
    [SerializeField] private TriggerEventOnEnterZone[] _triggerEventOnEnterZones;
    [SerializeField] private TriggerEventOnExitZone[] _triggerEventOnExitZones;

    [Header("Dialogue Reactions")]
    [SerializeField] private DialogueOnEvent[] _dialogueOnEvents;
    [SerializeField] private DialogueEventRouter[] _dialogueEventRouters;

    [Header("Debug")]
    [SerializeField] private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    private void Reset()
    {
        CollectFromScene();
    }

    private void Start()
    {
        if (_collectOnStart)
            CollectFromScene();

        if (_initializeOnStart)
            InitializeAll();
    }

    [ContextMenu("Collect From Scene")]
    public void CollectFromScene()
    {
        _scenarioStates = FindObjectsOfType<ScenarioStateService>(true);
        _inventories = FindObjectsOfType<InventoryService>(true);
        _interactionUis = FindObjectsOfType<InteractionUIService>(true);
        _subtitleUis = FindObjectsOfType<SubtitleUIService>(true);
        _dialogueServices = FindObjectsOfType<DialogueService>(true);

        _characters = FindObjectsOfType<FpsCharacter>(true);
        _playerLooks = FindObjectsOfType<PlayerLook>(true);
        _cameraZooms = FindObjectsOfType<CameraZoomController>(true);
        _playerInteractors = FindObjectsOfType<PlayerInteractor>(true);
        _flashlights = FindObjectsOfType<FlashlightController>(true);
        _cameraMotions = FindObjectsOfType<CameraMotionController>(true);
        _flashlightSways = FindObjectsOfType<FlashlightSwayController>(true);

        _interactables = FindObjectsOfType<InteractableObject>(true);

        _enableOnEvents = FindObjectsOfType<EnableOnEvent>(true);
        _disableOnEvents = FindObjectsOfType<DisableOnEvent>(true);
        _animatorTriggerOnEvents = FindObjectsOfType<AnimatorTriggerOnEvent>(true);
        _playSoundOnEvents = FindObjectsOfType<PlaySoundOnEvent>(true);
        _triggerEventAfterDelays = FindObjectsOfType<TriggerEventAfterDelay>(true);
        _triggerEventOnStarts = FindObjectsOfType<TriggerEventOnStart>(true);
        _triggerEventOnEnterZones = FindObjectsOfType<TriggerEventOnEnterZone>(true);
        _triggerEventOnExitZones = FindObjectsOfType<TriggerEventOnExitZone>(true);

        _dialogueOnEvents = FindObjectsOfType<DialogueOnEvent>(true);
        _dialogueEventRouters = FindObjectsOfType<DialogueEventRouter>(true);
    }

    [ContextMenu("Initialize All")]
    public void InitializeAll()
    {
        foreach (ScenarioStateService scenarioState in _scenarioStates)
            if (scenarioState != null)
                scenarioState.Initialize();

        foreach (InventoryService inventory in _inventories)
            if (inventory != null)
                inventory.Initialize();

        foreach (InteractionUIService interactionUi in _interactionUis)
            if (interactionUi != null)
                interactionUi.Initialize();

        foreach (SubtitleUIService subtitleUi in _subtitleUis)
            if (subtitleUi != null)
                subtitleUi.Initialize();

        foreach (DialogueService dialogueService in _dialogueServices)
            if (dialogueService != null)
                dialogueService.Initialize();

        foreach (FpsCharacter character in _characters)
            if (character != null)
                character.Initialize();

        foreach (PlayerLook playerLook in _playerLooks)
            if (playerLook != null)
                playerLook.Initialize();

        foreach (CameraZoomController cameraZoom in _cameraZooms)
            if (cameraZoom != null)
                cameraZoom.Initialize();

        foreach (PlayerInteractor playerInteractor in _playerInteractors)
            if (playerInteractor != null)
                playerInteractor.Initialize();

        foreach (FlashlightController flashlight in _flashlights)
            if (flashlight != null)
                flashlight.Initialize();

        foreach (CameraMotionController cameraMotion in _cameraMotions)
            if (cameraMotion != null)
                cameraMotion.Initialize();

        foreach (FlashlightSwayController flashlightSway in _flashlightSways)
            if (flashlightSway != null)
                flashlightSway.Initialize();

        foreach (InteractableObject interactable in _interactables)
            if (interactable != null)
                interactable.Initialize();

        // Event reaction components подписываются на GameEventBus здесь.
        foreach (EnableOnEvent enableOnEvent in _enableOnEvents)
            if (enableOnEvent != null)
                enableOnEvent.Initialize();

        foreach (DisableOnEvent disableOnEvent in _disableOnEvents)
            if (disableOnEvent != null)
                disableOnEvent.Initialize();

        foreach (AnimatorTriggerOnEvent animatorTriggerOnEvent in _animatorTriggerOnEvents)
            if (animatorTriggerOnEvent != null)
                animatorTriggerOnEvent.Initialize();

        foreach (PlaySoundOnEvent playSoundOnEvent in _playSoundOnEvents)
            if (playSoundOnEvent != null)
                playSoundOnEvent.Initialize();

        foreach (TriggerEventAfterDelay triggerEventAfterDelay in _triggerEventAfterDelays)
            if (triggerEventAfterDelay != null)
                triggerEventAfterDelay.Initialize();

        foreach (TriggerEventOnEnterZone triggerEventOnEnterZone in _triggerEventOnEnterZones)
            if (triggerEventOnEnterZone != null)
                triggerEventOnEnterZone.Initialize();

        foreach (TriggerEventOnExitZone triggerEventOnExitZone in _triggerEventOnExitZones)
            if (triggerEventOnExitZone != null)
                triggerEventOnExitZone.Initialize();

        foreach (DialogueOnEvent dialogueOnEvent in _dialogueOnEvents)
            if (dialogueOnEvent != null)
                dialogueOnEvent.Initialize();

        foreach (DialogueEventRouter dialogueEventRouter in _dialogueEventRouters)
            if (dialogueEventRouter != null)
                dialogueEventRouter.Initialize();

        // Start-триггеры инициализируются последними, чтобы все слушатели уже успели подписаться на GameEventBus.
        foreach (TriggerEventOnStart triggerEventOnStart in _triggerEventOnStarts)
            if (triggerEventOnStart != null)
                triggerEventOnStart.Initialize();

        _isInitialized = true;
    }
}
