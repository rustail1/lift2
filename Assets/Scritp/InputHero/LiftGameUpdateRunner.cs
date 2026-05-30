using UnityEngine;

[DisallowMultipleComponent]
public class LiftGameUpdateRunner : MonoBehaviour
{
    [Header("Auto")]
    [SerializeField] private bool _collectOnStart = true;

    [Header("UI")]
    [SerializeField] private InteractionUIService[] _interactionUis;
    [SerializeField] private SubtitleUIService[] _subtitleUis;
    [SerializeField] private DialogueService[] _dialogueServices;

    [Header("Player Components")]
    [SerializeField] private PlayerLook[] _playerLooks;
    [SerializeField] private FpsCharacter[] _characters;
    [SerializeField] private CameraZoomController[] _cameraZooms;
    [SerializeField] private FlashlightController[] _flashlights;
    [SerializeField] private CameraMotionController[] _cameraMotions;
    [SerializeField] private FlashlightSwayController[] _flashlightSways;
    [SerializeField] private PlayerInteractor[] _playerInteractors;

    [Header("World Components")]
    [SerializeField] private InteractableObject[] _interactables;

    [Header("Event Reactions")]
    [SerializeField] private TriggerEventAfterDelay[] _triggerEventAfterDelays;
    [SerializeField] private TriggerEventOnStart[] _triggerEventOnStarts;

    private void Reset()
    {
        CollectFromScene();
    }

    private void Start()
    {
        if (_collectOnStart)
            CollectFromScene();
    }

    private void Update()
    {
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

        // 9. В конце обновляем raycast интеракции и UI подсказок.
        foreach (PlayerInteractor playerInteractor in _playerInteractors)
            if (playerInteractor != null)
                playerInteractor.Tick();
    }

    [ContextMenu("Collect From Scene")]
    public void CollectFromScene()
    {
        _interactionUis = FindObjectsOfType<InteractionUIService>(true);
        _subtitleUis = FindObjectsOfType<SubtitleUIService>(true);
        _dialogueServices = FindObjectsOfType<DialogueService>(true);

        _playerLooks = FindObjectsOfType<PlayerLook>(true);
        _characters = FindObjectsOfType<FpsCharacter>(true);
        _cameraZooms = FindObjectsOfType<CameraZoomController>(true);
        _flashlights = FindObjectsOfType<FlashlightController>(true);
        _cameraMotions = FindObjectsOfType<CameraMotionController>(true);
        _flashlightSways = FindObjectsOfType<FlashlightSwayController>(true);
        _playerInteractors = FindObjectsOfType<PlayerInteractor>(true);

        _interactables = FindObjectsOfType<InteractableObject>(true);
        _triggerEventAfterDelays = FindObjectsOfType<TriggerEventAfterDelay>(true);
        _triggerEventOnStarts = FindObjectsOfType<TriggerEventOnStart>(true);
    }
}
