using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InventoryService : MonoBehaviour
{
    public static InventoryService Instance { get; private set; }

    public const string Screwdriver = "Screwdriver";
    public const string Flashlight = "Flashlight";
    public const string Fuse = "Fuse";
    public const string Manual = "Manual";
    public const string Key = "Key";

    [Header("Start Items")]
    [SerializeField] private string[] _startItems;

    [Header("Debug Flags")]
    [Tooltip("Только для просмотра и быстрого теста в Inspector.")]
    [SerializeField] private bool _hasScrewdriver;
    [Tooltip("Только для просмотра и быстрого теста в Inspector.")]
    [SerializeField] private bool _hasFlashlight;
    [Tooltip("Только для просмотра и быстрого теста в Inspector.")]
    [SerializeField] private bool _hasFuse;
    [Tooltip("Только для просмотра и быстрого теста в Inspector.")]
    [SerializeField] private bool _hasManual;
    [Tooltip("Только для просмотра и быстрого теста в Inspector.")]
    [SerializeField] private bool _hasKey;

    [Header("Debug Items")]
    [SerializeField] private List<string> _itemsDebug = new List<string>();

    private readonly HashSet<string> _items = new HashSet<string>();

    public bool HasScrewdriver => HasItem(Screwdriver);
    public bool HasFlashlight => HasItem(Flashlight);
    public bool HasFuse => HasItem(Fuse);
    public bool HasManual => HasItem(Manual);
    public bool HasKey => HasItem(Key);

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        Instance = this;
        _items.Clear();

        if (_startItems != null)
        {
            for (int i = 0; i < _startItems.Length; i++)
                AddItemSilently(_startItems[i]);
        }

        if (_hasScrewdriver) AddItemSilently(Screwdriver);
        if (_hasFlashlight) AddItemSilently(Flashlight);
        if (_hasFuse) AddItemSilently(Fuse);
        if (_hasManual) AddItemSilently(Manual);
        if (_hasKey) AddItemSilently(Key);

        SyncDebugState();
    }

    public void AddItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        if (_items.Add(itemId))
        {
            SyncDebugState();
            GameEventBus.Trigger("ItemAdded_" + itemId);
        }
    }

    public bool HasItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return true;

        return _items.Contains(itemId);
    }

    public void RemoveItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        if (_items.Remove(itemId))
        {
            SyncDebugState();
            GameEventBus.Trigger("ItemRemoved_" + itemId);
        }
    }

    private void AddItemSilently(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        _items.Add(itemId);
    }

    private void SyncDebugState()
    {
        _hasScrewdriver = _items.Contains(Screwdriver);
        _hasFlashlight = _items.Contains(Flashlight);
        _hasFuse = _items.Contains(Fuse);
        _hasManual = _items.Contains(Manual);
        _hasKey = _items.Contains(Key);

        _itemsDebug.Clear();
        foreach (string itemId in _items)
            _itemsDebug.Add(itemId);
    }
}
