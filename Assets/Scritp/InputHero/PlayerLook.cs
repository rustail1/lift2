using UnityEngine;

/// <summary>
/// Отвечает за поворот персонажа и наклон камеры по вводу мыши.
/// Используется вместе с CinemachineVirtualCamera, которая следует за CameraRoot.
/// </summary>
[DisallowMultipleComponent]
public class PlayerLook : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Точка, за которой следует виртуальная камера (CameraRoot в иерархии игрока).")]
    [SerializeField] private Transform _cameraRoot;

    [Header("Чувствительность мыши")]
    [Tooltip("Чувствительность мыши в градусах на единицу дельты. Чем больше значение, тем быстрее поворот.")]
    [SerializeField] private float _mouseSensitivity = 0.1f;

    [Header("Ограничения наклона (pitch)")]
    [Tooltip("Минимальный наклон камеры вниз (отрицательное значение).")]
    [SerializeField] private float _minPitch = -80f;

    [Tooltip("Максимальный наклон камеры вверх (положительное значение).")]
    [SerializeField] private float _maxPitch = 80f;

    /// <summary>
    /// Текущий накопленный угол наклона камеры по оси X (pitch).
    /// </summary>
    private float _pitch;

    /// <summary>
    /// Последняя дельта мыши, выставленная адаптером инпута.
    /// </summary>
    private Vector2 _lookDelta;

    /// <summary>
    /// Можно ли сейчас крутить камеру (например, выключаем в паузе/меню).
    /// </summary>
    private bool _lookEnabled = true;

    // Вызывается из LiftGameInitializer. Start специально не используется.
    public void Initialize()
    {
        // По умолчанию прячем курсор и фиксируем его в центре экрана.
        SetCursorLocked(true);
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick()
    {
        if (!_lookEnabled)
            return;

        ApplyLook();
    }

    /// <summary>
    /// Вызывается адаптером инпута. Сюда прилетает дельта мыши из Input System.
    /// </summary>
    public void SetLookDelta(Vector2 delta)
    {
        _lookDelta = delta;
    }

    /// <summary>
    /// Основная логика поворота камеры:
    /// - поворачиваем тело игрока по оси Y (yaw)
    /// - изменяем наклон камеры по оси X (pitch) с ограничением.
    /// </summary>
    private void ApplyLook()
    {
        if (_cameraRoot == null)
            return;

        // 1. Горизонтальный поворот (yaw) — крутим самого игрока.
        // Здесь не умножаем на deltaTime:
        // delta мыши уже зависит от FPS, а чувствительность подбираем экспериментально.
        float yaw = _lookDelta.x * _mouseSensitivity;
        transform.Rotate(Vector3.up * yaw);

        // 2. Вертикальный поворот (pitch) — наклон камеры.
        float pitchDelta = -_lookDelta.y * _mouseSensitivity;
        _pitch = Mathf.Clamp(_pitch + pitchDelta, _minPitch, _maxPitch);

        _cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    /// <summary>
    /// Включить/выключить возможность поворота камеры.
    /// </summary>
    public void SetLookEnabled(bool enabled)
    {
        _lookEnabled = enabled;
        SetCursorLocked(enabled);
    }

    /// <summary>
    /// Заблокировать/разблокировать курсор мыши и спрятать его.
    /// </summary>
    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
