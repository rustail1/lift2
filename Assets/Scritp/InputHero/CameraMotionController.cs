using UnityEngine;

/// <summary>
/// Добавляет небольшое иммерсивное покачивание камеры:
/// - лёгкое дыхание, когда игрок стоит;
/// - более заметное покачивание при ходьбе.
///
/// Важно для текущей архитектуры:
/// Awake/Update здесь специально не используются.
/// Initialize() вызывает LiftGameInitializer.
/// Tick() вызывает LiftGameUpdateRunner.
///
/// Рекомендуемая схема для Cinemachine:
/// Player
/// └── CameraRoot              // его крутит PlayerLook и двигает FpsCharacter при приседе
///     └── CameraMotionTarget  // его двигает этот скрипт, и на него смотрит/следует Cinemachine
///
/// Не вешай этот скрипт на тот же Transform, который двигает FpsCharacter для приседа,
/// иначе покачивание и присед будут бороться за localPosition.
/// </summary>
[DisallowMultipleComponent]
public class CameraMotionController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform, к которому применяется покачивание. Лучше отдельный дочерний объект внутри CameraRoot, например CameraMotionTarget / LookTarget.")]
    [SerializeField] private Transform _target;

    [Tooltip("Ссылка на FpsCharacter нужна, чтобы понимать, идёт игрок или стоит.")]
    [SerializeField] private FpsCharacter _character;

    [Header("Idle Breathing")]
    [Tooltip("Включить лёгкое дыхание камеры, когда игрок стоит.")]
    [SerializeField] private bool _enableIdleBreathing = true;

    [SerializeField] private float _idlePositionAmount = 0.015f;
    [SerializeField] private float _idleRotationAmount = 0.25f;
    [SerializeField] private float _idleSpeed = 1.2f;

    [Header("Walk Bob")]
    [Tooltip("Включить покачивание камеры при ходьбе.")]
    [SerializeField] private bool _enableWalkBob = true;

    [SerializeField] private float _walkVerticalAmount = 0.045f;
    [SerializeField] private float _walkHorizontalAmount = 0.025f;
    [SerializeField] private float _walkRotationAmount = 0.8f;
    [SerializeField] private float _walkSpeed = 7.5f;

    [Header("Smoothing")]
    [SerializeField] private float _positionSmooth = 12f;
    [SerializeField] private float _rotationSmooth = 12f;

    [Header("Debug")]
    [Tooltip("Только для просмотра в Inspector: сейчас считается, что игрок движется.")]
    [SerializeField] private bool _isMoving;

    [Tooltip("Только для просмотра в Inspector: сила ввода движения от 0 до 1.")]
    [SerializeField] private float _moveAmount;

    private Vector3 _baseLocalPosition;
    private Quaternion _baseLocalRotation;
    private float _idleTimer;
    private float _walkTimer;

    public bool IsMoving => _isMoving;
    public float MoveAmount => _moveAmount;

    public void Initialize()
    {
        if (_target == null)
            _target = transform;

        if (_character == null)
            _character = GetComponentInParent<FpsCharacter>();

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

        Vector3 targetPositionOffset = Vector3.zero;
        Vector3 targetEulerOffset = Vector3.zero;

        if (_isMoving && _enableWalkBob)
        {
            _walkTimer += deltaTime * _walkSpeed;

            // Вертикаль делаем через Abs(Sin), чтобы было ощущение шагов: вверх-вниз на каждом шаге.
            float vertical = Mathf.Abs(Mathf.Sin(_walkTimer)) * _walkVerticalAmount;
            float horizontal = Mathf.Sin(_walkTimer * 0.5f) * _walkHorizontalAmount;
            float roll = Mathf.Sin(_walkTimer) * _walkRotationAmount;

            targetPositionOffset = new Vector3(horizontal, vertical, 0f) * _moveAmount;
            targetEulerOffset = new Vector3(0f, 0f, -roll) * _moveAmount;
        }
        else if (_enableIdleBreathing)
        {
            _idleTimer += deltaTime * _idleSpeed;

            float breathe = Mathf.Sin(_idleTimer);
            float breatheSide = Mathf.Sin(_idleTimer * 0.55f);

            targetPositionOffset = new Vector3(
                breatheSide * _idlePositionAmount * 0.35f,
                breathe * _idlePositionAmount,
                0f
            );

            targetEulerOffset = new Vector3(
                breathe * _idleRotationAmount,
                breatheSide * _idleRotationAmount * 0.35f,
                0f
            );
        }
        else
        {
            // Когда стоим без idle-breathing, постепенно возвращаемся в базовое положение.
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
