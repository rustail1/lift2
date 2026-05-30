using System.Reflection;
using UnityEngine;

[DisallowMultipleComponent]
public class CameraZoomController : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Обычная Main Camera. Нужна как fallback, если Cinemachine камера не указана.")]
    [SerializeField] private Camera _camera;

    [Tooltip("Сюда можно перетащить Cinemachine Camera / Cinemachine Virtual Camera. Скрипт меняет её Lens FOV через reflection, поэтому не зависит напрямую от версии Cinemachine.")]
    [SerializeField] private MonoBehaviour _cinemachineCamera;

    [Header("Zoom")]
    [SerializeField] private float _normalFov = 60f;
    [SerializeField] private float _zoomFov = 35f;
    [SerializeField] private float _zoomSpeed = 12f;

    [Header("Debug")]
    [SerializeField] private bool _isZooming;
    [SerializeField] private float _currentFov;

    public bool IsZooming => _isZooming;

    private void Reset()
    {
        if (_camera == null)
            _camera = Camera.main != null ? Camera.main : GetComponent<Camera>();
    }

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        if (_camera == null)
            _camera = Camera.main != null ? Camera.main : GetComponent<Camera>();

        // Важно:
        // НЕ перезаписываем _normalFov из Cinemachine.
        // _normalFov должен оставаться таким, каким ты выставил его в Inspector.
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick()
    {
        float targetFov = _isZooming ? _zoomFov : _normalFov;

        if (_cinemachineCamera != null && TryGetCinemachineFov(out float currentCmFov))
        {
            _currentFov = currentCmFov;

            float newFov = Mathf.Lerp(
                currentCmFov,
                targetFov,
                Time.deltaTime * _zoomSpeed
            );

            TrySetCinemachineFov(newFov);
            return;
        }

        // Fallback для варианта без Cinemachine.
        if (_camera == null)
            return;

        _currentFov = _camera.fieldOfView;

        _camera.fieldOfView = Mathf.Lerp(
            _camera.fieldOfView,
            targetFov,
            Time.deltaTime * _zoomSpeed
        );
    }

    // Hold-режим: пока кнопка зажата, zoom включен; отпустил кнопку — zoom выключен.
    public void SetZoom(bool zoom)
    {
        _isZooming = zoom;
    }

    private bool TryGetCinemachineFov(out float fov)
    {
        fov = 0f;

        if (_cinemachineCamera == null)
            return false;

        object lens = GetLensObject();
        if (lens == null)
            return false;

        System.Type lensType = lens.GetType();

        FieldInfo field = lensType.GetField("FieldOfView");
        if (field != null && field.FieldType == typeof(float))
        {
            fov = (float)field.GetValue(lens);
            return true;
        }

        PropertyInfo property = lensType.GetProperty("FieldOfView");
        if (property != null && property.PropertyType == typeof(float))
        {
            fov = (float)property.GetValue(lens);
            return true;
        }

        return false;
    }

    private bool TrySetCinemachineFov(float fov)
    {
        if (_cinemachineCamera == null)
            return false;

        System.Type cmType = _cinemachineCamera.GetType();
        object lens = GetLensObject();

        if (lens == null)
            return false;

        System.Type lensType = lens.GetType();
        bool changed = false;

        FieldInfo fovField = lensType.GetField("FieldOfView");
        if (fovField != null && fovField.FieldType == typeof(float))
        {
            fovField.SetValue(lens, fov);
            changed = true;
        }
        else
        {
            PropertyInfo fovProperty = lensType.GetProperty("FieldOfView");
            if (fovProperty != null && fovProperty.CanWrite && fovProperty.PropertyType == typeof(float))
            {
                fovProperty.SetValue(lens, fov);
                changed = true;
            }
        }

        if (!changed)
            return false;

        // Cinemachine 3: свойство Lens.
        PropertyInfo lensProperty = cmType.GetProperty("Lens");
        if (lensProperty != null && lensProperty.CanWrite)
        {
            lensProperty.SetValue(_cinemachineCamera, lens);
            return true;
        }

        // Cinemachine 2: поле m_Lens.
        FieldInfo lensField = cmType.GetField("m_Lens");
        if (lensField != null)
        {
            lensField.SetValue(_cinemachineCamera, lens);
            return true;
        }

        return false;
    }

    private object GetLensObject()
    {
        if (_cinemachineCamera == null)
            return null;

        System.Type cmType = _cinemachineCamera.GetType();

        // Cinemachine 3 обычно использует свойство Lens.
        PropertyInfo lensProperty = cmType.GetProperty("Lens");
        if (lensProperty != null)
            return lensProperty.GetValue(_cinemachineCamera);

        // Старый CinemachineVirtualCamera обычно использует поле m_Lens.
        FieldInfo lensField = cmType.GetField("m_Lens");
        if (lensField != null)
            return lensField.GetValue(_cinemachineCamera);

        return null;
    }
}
