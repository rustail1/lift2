using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class FpsCharacter : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _crouchSpeed = 1.1f;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -9.81f;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask _groundMask;

    [Header("Crouch")]
    [SerializeField] private Transform _cameraRoot;
    [SerializeField] private float _standingHeight = 1.8f;
    [SerializeField] private float _crouchHeight = 1.1f;
    [SerializeField] private Vector3 _standingCenter = Vector3.zero;
    [SerializeField] private Vector3 _crouchCenter = new Vector3(0f, -0.35f, 0f);
    [SerializeField] private float _standingCameraY = 1.6f;
    [SerializeField] private float _crouchCameraY = 1.0f;
    [SerializeField] private float _crouchTransitionSpeed = 10f;

    [Header("Events")]
    public AK.Wwise.Event myFootstep;

    [Header("Debug State")]
    [Tooltip("Только для просмотра в Inspector: сейчас игрок присел или нет.")]
    [SerializeField] private bool _isCrouching;
    [Tooltip("Только для просмотра в Inspector: игрок касается земли или нет.")]
    [SerializeField] private bool _isGrounded;
    [Tooltip("Только для просмотра в Inspector: движение игрока включено или нет.")]
    [SerializeField] private bool _movementEnabled = true;

    private CharacterController _controller;
    private Vector2 _moveInput;
    private float _verticalVelocity;

    public bool IsGrounded => _isGrounded;
    public bool IsCrouching => _isCrouching;
    public bool MovementEnabled => _movementEnabled;
    public Vector2 MoveInput => _moveInput;
    public float MoveInputMagnitude => _moveInput.magnitude;

    //wwise
    private bool footstepIsPlaying = false;
    private float lastFootstepTime = 0f;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        if (_controller == null)
            _controller = GetComponent<CharacterController>();

        lastFootstepTime = Time.time;
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick()
    {
        if (_controller == null)
            Initialize();

        if (_controller == null)
            return;

        UpdateGroundedState();
        ApplyGravity();
        ApplyCrouch();
        ApplyMovement();
    }

    public void SetMoveInput(Vector2 move)
    {
        _moveInput = Vector2.ClampMagnitude(move, 1f);
    }

    // Hold-присед: пока кнопка зажата — игрок сидит, отпустил — встал.
    public void SetCrouch(bool crouch)
    {
        _isCrouching = crouch;
    }

    // Оставлено на случай, если потом понадобится toggle-присед.
    public void ToggleCrouch()
    {
        _isCrouching = !_isCrouching;
    }

    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;

        if (!enabled)
            _moveInput = Vector2.zero;
    }

    private void UpdateGroundedState()
    {
        if (_groundCheck != null)
        {
            _isGrounded = Physics.CheckSphere(
                _groundCheck.position,
                _groundCheckRadius,
                _groundMask
            );
        }
        else
        {
            _isGrounded = _controller.isGrounded;
        }

        if (_isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
    }

    private void ApplyGravity()
    {
        _verticalVelocity += _gravity * Time.deltaTime;
    }

    private void ApplyCrouch()
    {
        float targetHeight = _isCrouching ? _crouchHeight : _standingHeight;
        Vector3 targetCenter = _isCrouching ? _crouchCenter : _standingCenter;

        _controller.height = Mathf.Lerp(
            _controller.height,
            targetHeight,
            Time.deltaTime * _crouchTransitionSpeed
        );

        _controller.center = Vector3.Lerp(
            _controller.center,
            targetCenter,
            Time.deltaTime * _crouchTransitionSpeed
        );

        if (_cameraRoot != null)
        {
            Vector3 localPos = _cameraRoot.localPosition;
            float targetCameraY = _isCrouching ? _crouchCameraY : _standingCameraY;

            localPos.y = Mathf.Lerp(
                localPos.y,
                targetCameraY,
                Time.deltaTime * _crouchTransitionSpeed
            );

            _cameraRoot.localPosition = localPos;
        }
    }

    private void ApplyMovement()
    {
        Vector3 inputDir = _movementEnabled
            ? new Vector3(_moveInput.x, 0f, _moveInput.y)
            : Vector3.zero;

        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        float speed = _isCrouching ? _crouchSpeed : _walkSpeed;
        Vector3 horizontal = transform.TransformDirection(inputDir) * speed;

        Vector3 motion = horizontal;
        motion.y = _verticalVelocity;

        _controller.Move(motion * Time.deltaTime);

        if (!footstepIsPlaying)
        {
            myFootstep.Post(gameObject);
            lastFootstepTime = Time.time;
            footstepIsPlaying = true;
        }
        else
        {
            if (_walkSpeed > 1)
            {
                if (Time.time - lastFootstepTime > 500 / _walkSpeed * Time.time)
                {
                    footstepIsPlaying = false;
                }
            }
        }

    }

    private void OnDrawGizmosSelected()
    {
        if (_groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
    }
}
