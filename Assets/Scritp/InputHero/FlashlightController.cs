using UnityEngine;

[DisallowMultipleComponent]
public class FlashlightController : MonoBehaviour
{
    [SerializeField] private Light _flashlight;

    [Header("Inventory Link")]
    [Tooltip("Если включено, наличие фонаря берётся из InventoryService по ID предмета.")]
    [SerializeField] private bool _syncWithInventory;

    [Tooltip("ID предмета фонаря в InventoryService.")]
    [SerializeField] private string _flashlightItemId = "Flashlight";

    [Header("Debug State")]
    [Tooltip("Есть ли у игрока фонарь. Можно включить в Inspector для теста.")]
    [SerializeField] private bool _hasFlashlight = true;
    [Tooltip("Только для просмотра в Inspector: фонарь сейчас включен или нет.")]
    [SerializeField] private bool _isOn;

    public bool HasFlashlight => _hasFlashlight;
    public bool IsOn => _isOn;

    private void Reset()
    {
        _flashlight = GetComponentInChildren<Light>();
        _hasFlashlight = true;
        _isOn = false;
    }

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        SyncFromInventory();
        ApplyState();
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick()
    {
        SyncFromInventory();
        ApplyState();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyState();
    }
#endif

    public void GiveFlashlight()
    {
        _hasFlashlight = true;
        ApplyState();
    }

    public void RemoveFlashlight()
    {
        _hasFlashlight = false;
        _isOn = false;
        ApplyState();
    }

    // Hold-фонарик: пока кнопка зажата — фонарь включен, отпустил — выключен.
    public void SetFlashlight(bool enabled)
    {
        if (!_hasFlashlight)
        {
            _isOn = false;
            ApplyState();
            return;
        }

        _isOn = enabled;
        ApplyState();
    }

    // Оставлено на случай, если потом понадобится toggle-фонарик.
    public void Toggle()
    {
        if (!_hasFlashlight) return;

        _isOn = !_isOn;
        ApplyState();
    }

    private void SyncFromInventory()
    {
        if (!_syncWithInventory)
            return;

        InventoryService inventory = InventoryService.Instance;
        _hasFlashlight = inventory != null && inventory.HasItem(_flashlightItemId);

        if (!_hasFlashlight)
            _isOn = false;
    }

    private void ApplyState()
    {
        if (_flashlight != null)
            _flashlight.enabled = _hasFlashlight && _isOn;
    }
}
