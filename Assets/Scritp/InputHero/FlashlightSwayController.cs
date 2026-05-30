using UnityEngine;

/// <summary>
/// Добавляет небольшое покачивание фонарика:
/// - мягкое покачивание, когда игрок стоит;
/// - более заметное покачивание при ходьбе.
///
/// Awake/Update здесь специально не используются.
/// Initialize() вызывает LiftGameInitializer.
/// Tick() вызывает LiftGameUpdateRunner.
///
/// Вешать лучше на родительский объект фонаря, например:
/// Main Camera
/// └── FlashlightRoot      // сюда FlashlightSwayController
///     └── Spot Light
/// </summary>
[DisallowMultipleComponent]
public class FlashlightSwayController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform фонарика, который будет покачиваться. Если пусто — используется Transform этого объекта.")]
    [SerializeField] private Transform _target;

    [Tooltip("Ссылка на FpsCharacter нужна, чтобы понимать, идёт игрок или стоит.")]
    [SerializeField] private FpsCharacter _character;

    [Tooltip("Опционально: если указан FlashlightController, покачивание будет сильнее только когда фонарь включён.")]
    [SerializeField] private FlashlightController _flashlightController;

    [Header("Idle Sway")]
    [SerializeField] private bool _enableIdleSway = true;
    [SerializeField] private float _idlePositionAmount = 0.01f;
    [SerializeField] private float _idleRotationAmount = 0.45f;
    [SerializeField] private float _idleSpeed = 1.1f;

    [Header("Walk Sway")]
    [SerializeField] private bool _enableWalkSway = true;
    [SerializeField] private float _walkPositionAmount = 0.035f;
    [SerializeField] private float _walkRotationAmount = 2.2f;
    [SerializeField] private float _walkSpeed = 7.5f;

    [Header("Smoothing")]
    [SerializeField] private float _positionSmooth = 12f;
    [SerializeField] private float _rotationSmooth = 12f;

    [Header("Debug")]
    [SerializeField] private bool _isMoving;
    [SerializeField] private float _moveAmount;

    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _idleTimer;
    private float _walkTimer;

    public void Initialize()
    {
        if (_target == null)
            _target = transform;

        if (_character == null)
            _character = GetComponentInParent<FpsCharacter>();

        if (_flashlightController == null)
            _flashlightController = GetComponentInParent<FlashlightController>();

        if (_target == null)
            return;

        _baseLocalPosition = _target.localPosition;
        _baseLocalRotation = _target.localRotation;
    }

    public void Tick()
    {
        if (_target == null)
            return;

        float deltaTime = Time.deltaTime;

        _moveAmount = _character != null ? _character.MoveInputMagnitude : 0f;
        _isMoving = _moveAmount > 0.05f && (_character == null || _character.MovementEnabled);

        // Если фонарь выключен, покачивание оставляем, но чуть слабее.
        // Так объект не выглядит мёртвым, но и не болтается слишком сильно в темноте.
        float enabledMultiplier = _flashlightController == null || _flashlightController.IsOn ? 1f : 0.35f;

        Vector3 targetPositionOffset = Vector3.zero;
        Vector3 targetEulerOffset = Vector3.zero;

        if (_isMoving && _enableWalkSway)
        {
            _walkTimer += deltaTime * _walkSpeed;

            float x = Mathf.Sin(_walkTimer * 0.5f) * _walkPositionAmount;
            float y = Mathf.Abs(Mathf.Sin(_walkTimer)) * _walkPositionAmount;
            float roll = Mathf.Sin(_walkTimer) * _walkRotationAmount;
            float pitch = Mathf.Cos(_walkTimer) * _walkRotationAmount * 0.45f;

            targetPositionOffset = new Vector3(x, y, 0f) * _moveAmount * enabledMultiplier;
            targetEulerOffset = new Vector3(pitch, 0f, -roll) * _moveAmount * enabledMultiplier;
        }
        else if (_enableIdleSway)
        {
            _idleTimer += deltaTime * _idleSpeed;

            float breathe = Mathf.Sin(_idleTimer);
            float side = Mathf.Sin(_idleTimer * 0.7f);

            targetPositionOffset = new Vector3(
                side * _idlePositionAmount,
                breathe * _idlePositionAmount,
                0f
            ) * enabledMultiplier;

            targetEulerOffset = new Vector3(
                breathe * _idleRotationAmount,
                side * _idleRotationAmount,
                -side * _idleRotationAmount * 0.5f
            ) * enabledMultiplier;
        }
        else
        {
            _walkTimer = 0f;
        }

        Vector3 desiredPosition = _baseLocalPosition + targetPositionOffset;
        Quaternion desiredRotation = _baseLocalRotation * Quaternion.Euler(targetEulerOffset);

        _target.localPosition = Vector3.Lerp(
            _target.localPosition,
            desiredPosition,
            deltaTime * _positionSmooth
        );

        _target.localRotation = Quaternion.Slerp(
            _target.localRotation,
            desiredRotation,
            deltaTime * _rotationSmooth
        );
    }

    [ContextMenu("Rebind Base Pose")]
    public void RebindBasePose()
    {
        if (_target == null)
            _target = transform;

        _baseLocalPosition = _target.localPosition;
        _baseLocalRotation = _target.localRotation;
    }
}
