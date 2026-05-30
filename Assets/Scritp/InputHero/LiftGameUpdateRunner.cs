using System;
using UnityEngine;

[DisallowMultipleComponent]
public class LiftGameUpdateRunner : MonoBehaviour
{
    [Header("Auto")]
    [Tooltip("Если включено, менеджер пересоберёт ссылки при старте. Если выключено, он всё равно соберёт их один раз, если список ещё пустой.")]
    [SerializeField] private bool _collectOnStart = true;

    // Runtime-only массивы. Не сериализуем их, чтобы Unity Inspector не падал на больших списках во время Play Mode.
    private InteractionUIService[] _interactionUis = Array.Empty<InteractionUIService>();
    private SubtitleUIService[] _subtitleUis = Array.Empty<SubtitleUIService>();
    private DialogueService[] _dialogueServices = Array.Empty<DialogueService>();
    private DocumentViewerService[] _documentViewers = Array.Empty<DocumentViewerService>();
    private ScreenFadeService[] _screenFades = Array.Empty<ScreenFadeService>();
    private InspectionViewService[] _inspectionViewServices = Array.Empty<InspectionViewService>();

    private PlayerLook[] _playerLooks = Array.Empty<PlayerLook>();
    private FpsCharacter[] _characters = Array.Empty<FpsCharacter>();
    private CameraZoomController[] _cameraZooms = Array.Empty<CameraZoomController>();
    private FlashlightController[] _flashlights = Array.Empty<FlashlightController>();
    private CameraMotionController[] _cameraMotions = Array.Empty<CameraMotionController>();
    private FlashlightSwayController[] _flashlightSways = Array.Empty<FlashlightSwayController>();
    private PlayerInteractor[] _playerInteractors = Array.Empty<PlayerInteractor>();

    private InteractableObject[] _interactables = Array.Empty<InteractableObject>();
    private TriggerEventAfterDelay[] _triggerEventAfterDelays = Array.Empty<TriggerEventAfterDelay>();
    private TriggerEventOnStart[] _triggerEventOnStarts = Array.Empty<TriggerEventOnStart>();
    private EmergencyTimerService[] _emergencyTimers = Array.Empty<EmergencyTimerService>();
    private ElevatorEffectsService[] _elevatorEffects = Array.Empty<ElevatorEffectsService>();

    [Header("Debug")]
    [SerializeField] private bool _hasCollected;

    [Header("Collected Counts / UI")]
    [SerializeField] private int _interactionUisCount;
    [SerializeField] private int _subtitleUisCount;
    [SerializeField] private int _dialogueServicesCount;
    [SerializeField] private int _documentViewersCount;
    [SerializeField] private int _screenFadesCount;
    [SerializeField] private int _inspectionViewServicesCount;

    [Header("Collected Counts / Player")]
    [SerializeField] private int _playerLooksCount;
    [SerializeField] private int _charactersCount;
    [SerializeField] private int _cameraZoomsCount;
    [SerializeField] private int _flashlightsCount;
    [SerializeField] private int _cameraMotionsCount;
    [SerializeField] private int _flashlightSwaysCount;
    [SerializeField] private int _playerInteractorsCount;

    [Header("Collected Counts / World")]
    [SerializeField] private int _interactablesCount;
    [SerializeField] private int _triggerEventAfterDelaysCount;
    [SerializeField] private int _triggerEventOnStartsCount;
    [SerializeField] private int _emergencyTimersCount;
    [SerializeField] private int _elevatorEffectsCount;

    private void Reset()
    {
        CollectFromScene();
    }

    private void Start()
    {
        if (_collectOnStart || !_hasCollected)
            CollectFromScene();
    }

    private void Update()
    {
        if (!_hasCollected)
            CollectFromScene();

        float deltaTime = Time.deltaTime;

        // 1. UI timers: короткие сообщения, ошибки и т.п.
        foreach (InteractionUIService interactionUi in _interactionUis)
            if (interactionUi != null)
                interactionUi.Tick(deltaTime);

        // 1.5. Диалоги/интерком обновляются централизованно.
        foreach (DialogueService dialogueService in _dialogueServices)
            if (dialogueService != null)
                dialogueService.Tick(deltaTime);

        // 1.6. Субтитры печатают текст постепенно через общий UpdateRunner.
        foreach (SubtitleUIService subtitleUi in _subtitleUis)
            if (subtitleUi != null)
                subtitleUi.Tick(deltaTime);

        // 1.7. Документы закрываются через Esc/ЛКМ только через общий UpdateRunner.
        foreach (DocumentViewerService documentViewer in _documentViewers)
            if (documentViewer != null)
                documentViewer.Tick(deltaTime);

        // 1.8. Fade-переходы обновляются централизованно.
        foreach (ScreenFadeService screenFade in _screenFades)
            if (screenFade != null)
                screenFade.Tick(deltaTime);

        // 1.9. Inspect / close-up камера для панелей и пазлов.
        foreach (InspectionViewService inspectionViewService in _inspectionViewServices)
            if (inspectionViewService != null)
                inspectionViewService.Tick(deltaTime);

        // 2. Сначала поворот камеры/персонажа.
        foreach (PlayerLook playerLook in _playerLooks)
            if (playerLook != null)
                playerLook.Tick();

        // 3. Потом движение персонажа.
        foreach (FpsCharacter character in _characters)
            if (character != null)
                character.Tick();

        // 4. Потом плавный zoom.
        foreach (CameraZoomController cameraZoom in _cameraZooms)
            if (cameraZoom != null)
                cameraZoom.Tick();

        // 5. Фонарик. Нужен Tick, если он синхронизируется с InventoryService.
        foreach (FlashlightController flashlight in _flashlights)
            if (flashlight != null)
                flashlight.Tick();

        // 6. Покачивание камеры и фонарика после движения/zoom, чтобы оно было добавочным эффектом.
        foreach (CameraMotionController cameraMotion in _cameraMotions)
            if (cameraMotion != null)
                cameraMotion.Tick();

        foreach (FlashlightSwayController flashlightSway in _flashlightSways)
            if (flashlightSway != null)
                flashlightSway.Tick();

        // 7. Взаимодействия Hold / Mash обновляются централизованно.
        foreach (InteractableObject interactable in _interactables)
            if (interactable != null)
                interactable.InteractionTick(deltaTime);

        // 8. Отложенные события обновляются централизованно.
        foreach (TriggerEventAfterDelay triggerEventAfterDelay in _triggerEventAfterDelays)
            if (triggerEventAfterDelay != null)
                triggerEventAfterDelay.Tick(deltaTime);

        // 8.5. Стартовые события тоже тикаются здесь, чтобы они сработали уже после Initialize всех слушателей.
        foreach (TriggerEventOnStart triggerEventOnStart in _triggerEventOnStarts)
            if (triggerEventOnStart != null)
                triggerEventOnStart.Tick(deltaTime);

        // 8.6. Таймер аварии.
        foreach (EmergencyTimerService emergencyTimer in _emergencyTimers)
            if (emergencyTimer != null)
                emergencyTimer.Tick(deltaTime);

        // 8.7. Эффекты лифта: тряска, flicker.
        foreach (ElevatorEffectsService elevatorEffect in _elevatorEffects)
            if (elevatorEffect != null)
                elevatorEffect.Tick(deltaTime);

        // 9. В конце обновляем raycast интеракции и UI подсказок.
        foreach (PlayerInteractor playerInteractor in _playerInteractors)
            if (playerInteractor != null)
                playerInteractor.Tick();
    }

    [ContextMenu("Collect From Scene")]
    public void CollectFromScene()
    {
        _interactionUis = Collect<InteractionUIService>();
        _subtitleUis = Collect<SubtitleUIService>();
        _dialogueServices = Collect<DialogueService>();
        _documentViewers = Collect<DocumentViewerService>();
        _screenFades = Collect<ScreenFadeService>();
        _inspectionViewServices = Collect<InspectionViewService>();

        _playerLooks = Collect<PlayerLook>();
        _characters = Collect<FpsCharacter>();
        _cameraZooms = Collect<CameraZoomController>();
        _flashlights = Collect<FlashlightController>();
        _cameraMotions = Collect<CameraMotionController>();
        _flashlightSways = Collect<FlashlightSwayController>();
        _playerInteractors = Collect<PlayerInteractor>();

        _interactables = Collect<InteractableObject>();
        _triggerEventAfterDelays = Collect<TriggerEventAfterDelay>();
        _triggerEventOnStarts = Collect<TriggerEventOnStart>();
        _emergencyTimers = Collect<EmergencyTimerService>();
        _elevatorEffects = Collect<ElevatorEffectsService>();

        _hasCollected = true;
        RefreshCounts();
    }

    private static T[] Collect<T>() where T : UnityEngine.Object
    {
        T[] objects = FindObjectsOfType<T>(true);
        return objects ?? Array.Empty<T>();
    }

    private void RefreshCounts()
    {
        _interactionUisCount = _interactionUis.Length;
        _subtitleUisCount = _subtitleUis.Length;
        _dialogueServicesCount = _dialogueServices.Length;
        _documentViewersCount = _documentViewers.Length;
        _screenFadesCount = _screenFades.Length;
        _inspectionViewServicesCount = _inspectionViewServices.Length;

        _playerLooksCount = _playerLooks.Length;
        _charactersCount = _characters.Length;
        _cameraZoomsCount = _cameraZooms.Length;
        _flashlightsCount = _flashlights.Length;
        _cameraMotionsCount = _cameraMotions.Length;
        _flashlightSwaysCount = _flashlightSways.Length;
        _playerInteractorsCount = _playerInteractors.Length;

        _interactablesCount = _interactables.Length;
        _triggerEventAfterDelaysCount = _triggerEventAfterDelays.Length;
        _triggerEventOnStartsCount = _triggerEventOnStarts.Length;
        _emergencyTimersCount = _emergencyTimers.Length;
        _elevatorEffectsCount = _elevatorEffects.Length;
    }
}
