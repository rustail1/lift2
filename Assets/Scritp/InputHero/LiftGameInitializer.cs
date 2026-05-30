using System;
using UnityEngine;

[DisallowMultipleComponent]
public class LiftGameInitializer : MonoBehaviour
{
    [Header("Auto")]
    [Tooltip("Если включено, менеджер пересоберёт ссылки при старте. Если выключено, он всё равно соберёт их один раз, если список ещё пустой.")]
    [SerializeField] private bool _collectOnStart = true;
    [SerializeField] private bool _initializeOnStart = true;

    // Важно: эти массивы специально НЕ сериализуются.
    // Unity 6 иногда кидает NullReferenceException в Inspector/UIElements, когда во время Play Mode
    // отрисовывает большие SerializedObject списки, которые одновременно пересобираются через FindObjectsOfType.
    // Поэтому списки храним только runtime-полями, а в Inspector показываем только counts.
    private ScenarioStateService[] _scenarioStates = Array.Empty<ScenarioStateService>();
    private InventoryService[] _inventories = Array.Empty<InventoryService>();
    private InteractionUIService[] _interactionUis = Array.Empty<InteractionUIService>();
    private SubtitleUIService[] _subtitleUis = Array.Empty<SubtitleUIService>();
    private DialogueService[] _dialogueServices = Array.Empty<DialogueService>();
    private DocumentViewerService[] _documentViewers = Array.Empty<DocumentViewerService>();
    private ScreenFadeService[] _screenFades = Array.Empty<ScreenFadeService>();
    private PlayerControlLockService[] _playerControlLocks = Array.Empty<PlayerControlLockService>();
    private EmergencyTimerService[] _emergencyTimers = Array.Empty<EmergencyTimerService>();
    private ElevatorEffectsService[] _elevatorEffects = Array.Empty<ElevatorEffectsService>();
    private InspectionViewService[] _inspectionViewServices = Array.Empty<InspectionViewService>();

    private FpsCharacter[] _characters = Array.Empty<FpsCharacter>();
    private PlayerLook[] _playerLooks = Array.Empty<PlayerLook>();
    private CameraZoomController[] _cameraZooms = Array.Empty<CameraZoomController>();
    private PlayerInteractor[] _playerInteractors = Array.Empty<PlayerInteractor>();
    private FlashlightController[] _flashlights = Array.Empty<FlashlightController>();
    private CameraMotionController[] _cameraMotions = Array.Empty<CameraMotionController>();
    private FlashlightSwayController[] _flashlightSways = Array.Empty<FlashlightSwayController>();

    private InteractableObject[] _interactables = Array.Empty<InteractableObject>();
    private DocumentItem[] _documentItems = Array.Empty<DocumentItem>();
    private TransitionPoint[] _transitionPoints = Array.Empty<TransitionPoint>();
    private KeypadPanelController[] _keypadPanels = Array.Empty<KeypadPanelController>();
    private KeypadButton[] _keypadButtons = Array.Empty<KeypadButton>();
    private InspectableZone[] _inspectableZones = Array.Empty<InspectableZone>();

    private EnableOnEvent[] _enableOnEvents = Array.Empty<EnableOnEvent>();
    private DisableOnEvent[] _disableOnEvents = Array.Empty<DisableOnEvent>();
    private AnimatorTriggerOnEvent[] _animatorTriggerOnEvents = Array.Empty<AnimatorTriggerOnEvent>();
    private PlaySoundOnEvent[] _playSoundOnEvents = Array.Empty<PlaySoundOnEvent>();
    private TriggerEventAfterDelay[] _triggerEventAfterDelays = Array.Empty<TriggerEventAfterDelay>();
    private TriggerEventOnStart[] _triggerEventOnStarts = Array.Empty<TriggerEventOnStart>();
    private TriggerEventOnEnterZone[] _triggerEventOnEnterZones = Array.Empty<TriggerEventOnEnterZone>();
    private TriggerEventOnExitZone[] _triggerEventOnExitZones = Array.Empty<TriggerEventOnExitZone>();
    private SetLightOnEvent[] _setLightOnEvents = Array.Empty<SetLightOnEvent>();
    private SetTextOnEvent[] _setTextOnEvents = Array.Empty<SetTextOnEvent>();
    private PlayerControlLockOnEvent[] _playerControlLockOnEvents = Array.Empty<PlayerControlLockOnEvent>();

    private DialogueOnEvent[] _dialogueOnEvents = Array.Empty<DialogueOnEvent>();
    private DialogueEventRouter[] _dialogueEventRouters = Array.Empty<DialogueEventRouter>();

    [Header("Debug")]
    [SerializeField] private bool _isInitialized;
    [SerializeField] private bool _hasCollected;

    [Header("Collected Counts / Core")]
    [SerializeField] private int _scenarioStatesCount;
    [SerializeField] private int _inventoriesCount;
    [SerializeField] private int _interactionUisCount;
    [SerializeField] private int _subtitleUisCount;
    [SerializeField] private int _dialogueServicesCount;
    [SerializeField] private int _documentViewersCount;
    [SerializeField] private int _screenFadesCount;
    [SerializeField] private int _playerControlLocksCount;
    [SerializeField] private int _emergencyTimersCount;
    [SerializeField] private int _elevatorEffectsCount;
    [SerializeField] private int _inspectionViewServicesCount;

    [Header("Collected Counts / Player")]
    [SerializeField] private int _charactersCount;
    [SerializeField] private int _playerLooksCount;
    [SerializeField] private int _cameraZoomsCount;
    [SerializeField] private int _playerInteractorsCount;
    [SerializeField] private int _flashlightsCount;
    [SerializeField] private int _cameraMotionsCount;
    [SerializeField] private int _flashlightSwaysCount;

    [Header("Collected Counts / World")]
    [SerializeField] private int _interactablesCount;
    [SerializeField] private int _documentItemsCount;
    [SerializeField] private int _transitionPointsCount;
    [SerializeField] private int _keypadPanelsCount;
    [SerializeField] private int _keypadButtonsCount;
    [SerializeField] private int _inspectableZonesCount;

    [Header("Collected Counts / Events")]
    [SerializeField] private int _enableOnEventsCount;
    [SerializeField] private int _disableOnEventsCount;
    [SerializeField] private int _animatorTriggerOnEventsCount;
    [SerializeField] private int _playSoundOnEventsCount;
    [SerializeField] private int _triggerEventAfterDelaysCount;
    [SerializeField] private int _triggerEventOnStartsCount;
    [SerializeField] private int _triggerEventOnEnterZonesCount;
    [SerializeField] private int _triggerEventOnExitZonesCount;
    [SerializeField] private int _setLightOnEventsCount;
    [SerializeField] private int _setTextOnEventsCount;
    [SerializeField] private int _playerControlLockOnEventsCount;
    [SerializeField] private int _dialogueOnEventsCount;
    [SerializeField] private int _dialogueEventRoutersCount;

    public bool IsInitialized => _isInitialized;

    private void Reset()
    {
        CollectFromScene();
    }

    private void Start()
    {
        if (_collectOnStart || !_hasCollected)
            CollectFromScene();

        if (_initializeOnStart)
            InitializeAll();
    }

    [ContextMenu("Collect From Scene")]
    public void CollectFromScene()
    {
        _scenarioStates = Collect<ScenarioStateService>();
        _inventories = Collect<InventoryService>();
        _interactionUis = Collect<InteractionUIService>();
        _subtitleUis = Collect<SubtitleUIService>();
        _dialogueServices = Collect<DialogueService>();
        _documentViewers = Collect<DocumentViewerService>();
        _screenFades = Collect<ScreenFadeService>();
        _playerControlLocks = Collect<PlayerControlLockService>();
        _emergencyTimers = Collect<EmergencyTimerService>();
        _elevatorEffects = Collect<ElevatorEffectsService>();
        _inspectionViewServices = Collect<InspectionViewService>();

        _characters = Collect<FpsCharacter>();
        _playerLooks = Collect<PlayerLook>();
        _cameraZooms = Collect<CameraZoomController>();
        _playerInteractors = Collect<PlayerInteractor>();
        _flashlights = Collect<FlashlightController>();
        _cameraMotions = Collect<CameraMotionController>();
        _flashlightSways = Collect<FlashlightSwayController>();

        _interactables = Collect<InteractableObject>();
        _documentItems = Collect<DocumentItem>();
        _transitionPoints = Collect<TransitionPoint>();
        _keypadPanels = Collect<KeypadPanelController>();
        _keypadButtons = Collect<KeypadButton>();
        _inspectableZones = Collect<InspectableZone>();

        _enableOnEvents = Collect<EnableOnEvent>();
        _disableOnEvents = Collect<DisableOnEvent>();
        _animatorTriggerOnEvents = Collect<AnimatorTriggerOnEvent>();
        _playSoundOnEvents = Collect<PlaySoundOnEvent>();
        _triggerEventAfterDelays = Collect<TriggerEventAfterDelay>();
        _triggerEventOnStarts = Collect<TriggerEventOnStart>();
        _triggerEventOnEnterZones = Collect<TriggerEventOnEnterZone>();
        _triggerEventOnExitZones = Collect<TriggerEventOnExitZone>();
        _setLightOnEvents = Collect<SetLightOnEvent>();
        _setTextOnEvents = Collect<SetTextOnEvent>();
        _playerControlLockOnEvents = Collect<PlayerControlLockOnEvent>();

        _dialogueOnEvents = Collect<DialogueOnEvent>();
        _dialogueEventRouters = Collect<DialogueEventRouter>();

        _hasCollected = true;
        RefreshCounts();
    }

    [ContextMenu("Initialize All")]
    public void InitializeAll()
    {
        if (!_hasCollected)
            CollectFromScene();

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

        foreach (DocumentViewerService documentViewer in _documentViewers)
            if (documentViewer != null)
                documentViewer.Initialize();

        foreach (ScreenFadeService screenFade in _screenFades)
            if (screenFade != null)
                screenFade.Initialize();

        foreach (PlayerControlLockService playerControlLock in _playerControlLocks)
            if (playerControlLock != null)
                playerControlLock.Initialize();

        foreach (EmergencyTimerService emergencyTimer in _emergencyTimers)
            if (emergencyTimer != null)
                emergencyTimer.Initialize();

        foreach (ElevatorEffectsService elevatorEffect in _elevatorEffects)
            if (elevatorEffect != null)
                elevatorEffect.Initialize();

        foreach (InspectionViewService inspectionViewService in _inspectionViewServices)
            if (inspectionViewService != null)
                inspectionViewService.Initialize();

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

        foreach (DocumentItem documentItem in _documentItems)
            if (documentItem != null)
                documentItem.Initialize();

        foreach (TransitionPoint transitionPoint in _transitionPoints)
            if (transitionPoint != null)
                transitionPoint.Initialize();

        foreach (KeypadPanelController keypadPanel in _keypadPanels)
            if (keypadPanel != null)
                keypadPanel.Initialize();

        foreach (KeypadButton keypadButton in _keypadButtons)
            if (keypadButton != null)
                keypadButton.Initialize();

        foreach (InspectableZone inspectableZone in _inspectableZones)
            if (inspectableZone != null)
                inspectableZone.Initialize();

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

        foreach (SetLightOnEvent setLightOnEvent in _setLightOnEvents)
            if (setLightOnEvent != null)
                setLightOnEvent.Initialize();

        foreach (SetTextOnEvent setTextOnEvent in _setTextOnEvents)
            if (setTextOnEvent != null)
                setTextOnEvent.Initialize();

        foreach (PlayerControlLockOnEvent playerControlLockOnEvent in _playerControlLockOnEvents)
            if (playerControlLockOnEvent != null)
                playerControlLockOnEvent.Initialize();

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

    private static T[] Collect<T>() where T : UnityEngine.Object
    {
        T[] objects = FindObjectsOfType<T>(true);
        return objects ?? Array.Empty<T>();
    }

    private void RefreshCounts()
    {
        _scenarioStatesCount = _scenarioStates.Length;
        _inventoriesCount = _inventories.Length;
        _interactionUisCount = _interactionUis.Length;
        _subtitleUisCount = _subtitleUis.Length;
        _dialogueServicesCount = _dialogueServices.Length;
        _documentViewersCount = _documentViewers.Length;
        _screenFadesCount = _screenFades.Length;
        _playerControlLocksCount = _playerControlLocks.Length;
        _emergencyTimersCount = _emergencyTimers.Length;
        _elevatorEffectsCount = _elevatorEffects.Length;
        _inspectionViewServicesCount = _inspectionViewServices.Length;

        _charactersCount = _characters.Length;
        _playerLooksCount = _playerLooks.Length;
        _cameraZoomsCount = _cameraZooms.Length;
        _playerInteractorsCount = _playerInteractors.Length;
        _flashlightsCount = _flashlights.Length;
        _cameraMotionsCount = _cameraMotions.Length;
        _flashlightSwaysCount = _flashlightSways.Length;

        _interactablesCount = _interactables.Length;
        _documentItemsCount = _documentItems.Length;
        _transitionPointsCount = _transitionPoints.Length;
        _keypadPanelsCount = _keypadPanels.Length;
        _keypadButtonsCount = _keypadButtons.Length;
        _inspectableZonesCount = _inspectableZones.Length;

        _enableOnEventsCount = _enableOnEvents.Length;
        _disableOnEventsCount = _disableOnEvents.Length;
        _animatorTriggerOnEventsCount = _animatorTriggerOnEvents.Length;
        _playSoundOnEventsCount = _playSoundOnEvents.Length;
        _triggerEventAfterDelaysCount = _triggerEventAfterDelays.Length;
        _triggerEventOnStartsCount = _triggerEventOnStarts.Length;
        _triggerEventOnEnterZonesCount = _triggerEventOnEnterZones.Length;
        _triggerEventOnExitZonesCount = _triggerEventOnExitZones.Length;
        _setLightOnEventsCount = _setLightOnEvents.Length;
        _setTextOnEventsCount = _setTextOnEvents.Length;
        _playerControlLockOnEventsCount = _playerControlLockOnEvents.Length;
        _dialogueOnEventsCount = _dialogueOnEvents.Length;
        _dialogueEventRoutersCount = _dialogueEventRouters.Length;
    }
}
