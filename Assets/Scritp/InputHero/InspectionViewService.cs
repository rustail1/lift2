using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class InspectionViewService : MonoBehaviour
{
    public static InspectionViewService Instance { get; private set; }

    private enum InspectionState
    {
        None,
        Entering,
        Active,
        Exiting
    }

    [Header("Camera")]
    [Tooltip("Сюда перетащи Cinemachine Camera / Cinemachine Virtual Camera. Скрипт меняет Follow/LookAt через reflection и не зависит жёстко от версии Cinemachine.")]
    [SerializeField] private MonoBehaviour _cinemachineCamera;

    [Tooltip("Временная цель камеры для inspect mode. Если не указана, создаётся автоматически.")]
    [SerializeField] private Transform _inspectionProxy;

    [Tooltip("Настоящая Main Camera. Нужна, чтобы стартовать приближение из текущей позиции камеры.")]
    [SerializeField] private Camera _mainCamera;

    [Header("Player Lock")]
    [SerializeField] private FpsCharacter[] _characters;
    [SerializeField] private PlayerLook[] _playerLooks;
    [SerializeField] private bool _lockMovement = true;
    [SerializeField] private bool _lockLook = true;
    [SerializeField] private bool _keepInteractionEnabled = true;

    [Header("UI")]
    [Tooltip("Скрывать Crosshair_Normal / Crosshair_Active в inspect mode, чтобы прицел не мешал нажимать кнопки панели.")]
    [SerializeField] private bool _hideCrosshairWhileInspecting = true;

    [Tooltip("Можно оставить пустым — сервис найдётся автоматически через InteractionUIService.Instance / FindObjectOfType.")]
    [SerializeField] private InteractionUIService _interactionUIService;

    [Header("Input")]
    [Tooltip("Закрывать крупный план по Escape.")]
    [SerializeField] private bool _exitOnEscape = true;

    [Tooltip("Закрывать крупный план по правой кнопке мыши.")]
    [SerializeField] private bool _exitOnRightMouse = true;

    [Tooltip("В inspect mode курсор должен быть свободным и видимым, чтобы игрок мог нажимать маленькие кнопки на панели мышкой.")]
    [SerializeField] private bool _unlockCursorWhileInspecting = true;

    [Header("Transition")]
    [SerializeField] private float _defaultEnterSpeed = 8f;
    [SerializeField] private float _defaultExitSpeed = 10f;
    [SerializeField] private bool _useUnscaledTime;

    [Header("Debug")]
    [SerializeField] private bool _isInspecting;
    [SerializeField] private string _currentZoneName;
    [SerializeField] private InspectionState _state;

    private InspectableZone _currentZone;
    private Transform _targetViewPoint;
    private Transform _savedFollow;
    private Transform _savedLookAt;
    private float _enterSpeed;
    private float _exitSpeed;
    private bool _hasSavedCameraTargets;
    private CursorLockMode _savedCursorLockState;
    private bool _savedCursorVisible;

    public bool IsInspecting => _isInspecting;
    public InspectableZone CurrentZone => _currentZone;

    public void Initialize()
    {
        Instance = this;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_inspectionProxy == null)
        {
            GameObject proxy = new GameObject("InspectionCameraProxy");
            proxy.transform.SetParent(transform, false);
            _inspectionProxy = proxy.transform;
        }

        if (_characters == null || _characters.Length == 0)
            _characters = FindObjectsOfType<FpsCharacter>(true);

        if (_playerLooks == null || _playerLooks.Length == 0)
            _playerLooks = FindObjectsOfType<PlayerLook>(true);

        if (_interactionUIService == null)
            _interactionUIService = InteractionUIService.Instance != null
                ? InteractionUIService.Instance
                : FindObjectOfType<InteractionUIService>(true);

        _state = InspectionState.None;
        _isInspecting = false;
        _currentZoneName = string.Empty;
        _currentZone = null;
        _targetViewPoint = null;
    }

    public void Tick(float deltaTime)
    {
        float dt = _useUnscaledTime ? Time.unscaledDeltaTime : deltaTime;

        if (_state == InspectionState.Entering)
        {
            MoveProxyTo(_targetViewPoint, _enterSpeed, dt);

            if (IsProxyCloseTo(_targetViewPoint))
                _state = InspectionState.Active;
        }
        else if (_state == InspectionState.Active)
        {
            if (ShouldExitFromInput())
                ExitInspection();
        }
        else if (_state == InspectionState.Exiting)
        {
            if (_savedFollow != null)
            {
                MoveProxyTo(_savedFollow, _exitSpeed, dt);

                if (!IsProxyCloseTo(_savedFollow))
                    return;
            }

            FinishExit();
        }
    }

    public bool EnterInspection(InspectableZone zone, Transform viewPoint, float enterSpeed, float exitSpeed)
    {
        if (zone == null || viewPoint == null || _cinemachineCamera == null || _inspectionProxy == null)
            return false;

        // Если уже смотрим другую зону — сначала корректно выходим из неё.
        if (_isInspecting)
            ForceExitImmediate();

        _currentZone = zone;
        _targetViewPoint = viewPoint;
        _enterSpeed = enterSpeed > 0f ? enterSpeed : _defaultEnterSpeed;
        _exitSpeed = exitSpeed > 0f ? exitSpeed : _defaultExitSpeed;
        _currentZoneName = zone.name;

        _savedFollow = GetCinemachineTarget("Follow", "m_Follow");
        _savedLookAt = GetCinemachineTarget("LookAt", "m_LookAt");
        _hasSavedCameraTargets = true;
        _savedCursorLockState = Cursor.lockState;
        _savedCursorVisible = Cursor.visible;

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera != null)
        {
            _inspectionProxy.position = _mainCamera.transform.position;
            _inspectionProxy.rotation = _mainCamera.transform.rotation;
        }
        else if (_savedFollow != null)
        {
            _inspectionProxy.position = _savedFollow.position;
            _inspectionProxy.rotation = _savedFollow.rotation;
        }
        else
        {
            _inspectionProxy.position = viewPoint.position;
            _inspectionProxy.rotation = viewPoint.rotation;
        }

        SetCinemachineTarget("Follow", "m_Follow", _inspectionProxy);
        SetCinemachineTarget("LookAt", "m_LookAt", null);

        ApplyPlayerLock(true);
        SetInspectionUiState(true);
        _currentZone.OnInspectionEnteredInternal();

        _isInspecting = true;
        _state = InspectionState.Entering;
        return true;
    }

    public void ExitInspection()
    {
        if (!_isInspecting)
            return;

        _state = InspectionState.Exiting;
    }

    public void ForceExitImmediate()
    {
        if (!_isInspecting)
            return;

        FinishExit();
    }

    private void FinishExit()
    {
        if (_hasSavedCameraTargets)
        {
            SetCinemachineTarget("Follow", "m_Follow", _savedFollow);
            SetCinemachineTarget("LookAt", "m_LookAt", _savedLookAt);
        }

        if (_currentZone != null)
            _currentZone.OnInspectionExitedInternal();

        SetInspectionUiState(false);
        ApplyPlayerLock(false);

        // Если PlayerLook не назначен, он не сможет сам вернуть состояние курсора.
        // В таком случае возвращаем то состояние, которое было до входа в inspect mode.
        if (_unlockCursorWhileInspecting && (_playerLooks == null || _playerLooks.Length == 0))
        {
            Cursor.lockState = _savedCursorLockState;
            Cursor.visible = _savedCursorVisible;
        }

        _isInspecting = false;
        _state = InspectionState.None;
        _currentZoneName = string.Empty;
        _currentZone = null;
        _targetViewPoint = null;
        _savedFollow = null;
        _savedLookAt = null;
        _hasSavedCameraTargets = false;
    }

    private bool ShouldExitFromInput()
    {
        if (_exitOnEscape && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            return true;

        if (_exitOnRightMouse && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            return true;

        return false;
    }

    private void MoveProxyTo(Transform target, float speed, float deltaTime)
    {
        if (target == null || _inspectionProxy == null)
            return;

        float t = 1f - Mathf.Exp(-Mathf.Max(0.01f, speed) * deltaTime);
        _inspectionProxy.position = Vector3.Lerp(_inspectionProxy.position, target.position, t);
        _inspectionProxy.rotation = Quaternion.Slerp(_inspectionProxy.rotation, target.rotation, t);
    }

    private bool IsProxyCloseTo(Transform target)
    {
        if (target == null || _inspectionProxy == null)
            return true;

        float positionDistance = Vector3.Distance(_inspectionProxy.position, target.position);
        float angleDistance = Quaternion.Angle(_inspectionProxy.rotation, target.rotation);
        return positionDistance <= 0.01f && angleDistance <= 0.5f;
    }

    private void SetInspectionUiState(bool inspecting)
    {
        if (!_hideCrosshairWhileInspecting)
            return;

        if (_interactionUIService == null)
            _interactionUIService = InteractionUIService.Instance != null
                ? InteractionUIService.Instance
                : FindObjectOfType<InteractionUIService>(true);

        if (_interactionUIService != null)
            _interactionUIService.SetCrosshairSuppressed(inspecting);
    }

    private void ApplyPlayerLock(bool locked)
    {
        if (_lockMovement && _characters != null)
        {
            for (int i = 0; i < _characters.Length; i++)
                if (_characters[i] != null)
                    _characters[i].SetMovementEnabled(!locked);
        }

        if (_lockLook && _playerLooks != null)
        {
            for (int i = 0; i < _playerLooks.Length; i++)
                if (_playerLooks[i] != null)
                    _playerLooks[i].SetLookEnabled(!locked);
        }

        // В inspect mode взаимодействие должно остаться активным: игрок нажимает кнопки/тумблеры крупным планом.
        // Поэтому курсор в крупном плане лучше разблокировать и показать.
        if (locked && _keepInteractionEnabled && _unlockCursorWhileInspecting)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private Transform GetCinemachineTarget(string propertyName, string fieldName)
    {
        if (_cinemachineCamera == null)
            return null;

        System.Type type = _cinemachineCamera.GetType();

        PropertyInfo property = type.GetProperty(propertyName);
        if (property != null && typeof(Transform).IsAssignableFrom(property.PropertyType))
            return property.GetValue(_cinemachineCamera) as Transform;

        FieldInfo field = type.GetField(fieldName);
        if (field != null && typeof(Transform).IsAssignableFrom(field.FieldType))
            return field.GetValue(_cinemachineCamera) as Transform;

        return null;
    }

    private bool SetCinemachineTarget(string propertyName, string fieldName, Transform target)
    {
        if (_cinemachineCamera == null)
            return false;

        System.Type type = _cinemachineCamera.GetType();

        PropertyInfo property = type.GetProperty(propertyName);
        if (property != null && property.CanWrite && typeof(Transform).IsAssignableFrom(property.PropertyType))
        {
            property.SetValue(_cinemachineCamera, target);
            return true;
        }

        FieldInfo field = type.GetField(fieldName);
        if (field != null && typeof(Transform).IsAssignableFrom(field.FieldType))
        {
            field.SetValue(_cinemachineCamera, target);
            return true;
        }

        return false;
    }
}
