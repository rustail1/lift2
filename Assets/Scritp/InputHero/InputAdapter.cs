using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class InputAdapter : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private FpsCharacter _character;
    [SerializeField] private PlayerLook _playerLook;
    [SerializeField] private CameraZoomController _cameraZoom;
    [SerializeField] private PlayerInteractor _playerInteractor;
    [SerializeField] private FlashlightController _flashlightController;

    private void Reset()
    {
        if (_character == null)
            _character = GetComponent<FpsCharacter>();

        if (_playerLook == null)
            _playerLook = GetComponent<PlayerLook>();

        if (_cameraZoom == null)
            _cameraZoom = GetComponentInChildren<CameraZoomController>();

        if (_playerInteractor == null)
            _playerInteractor = GetComponentInChildren<PlayerInteractor>();

        if (_flashlightController == null)
            _flashlightController = GetComponentInChildren<FlashlightController>();
    }

    public void OnMove(InputValue value)
    {
        Vector2 move = value.Get<Vector2>();
        _character?.SetMoveInput(move);
    }

    public void OnLook(InputValue value)
    {
        Vector2 look = value.Get<Vector2>();
        _playerLook?.SetLookDelta(look);
    }

    public void OnCrouch(InputValue value)
    {
        // Hold-присед: пока C / Ctrl зажата — игрок сидит, отпустил — встал.
        _character?.SetCrouch(value.isPressed);
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
            _playerInteractor?.InteractDown();
        else
            _playerInteractor?.InteractUp();
    }

    public void OnZoom(InputValue value)
    {
        // Hold-zoom: пока правая кнопка мыши зажата — zoom включен, отпустил — выключен.
        _cameraZoom?.SetZoom(value.isPressed);
    }

    public void OnFlashlight(InputValue value)
    {
        // Toggle-фонарик: нажал F — включил, нажал F ещё раз — выключил.
        // Отпускание кнопки игнорируем, чтобы фонарь не выключался сразу после нажатия.
        if (value.isPressed)
            _flashlightController?.Toggle();
    }
}
