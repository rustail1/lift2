using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerControlLockService : MonoBehaviour
{
    public static PlayerControlLockService Instance { get; private set; }

    [Header("Targets")]
    [SerializeField] private FpsCharacter[] _characters;
    [SerializeField] private PlayerLook[] _playerLooks;
    [SerializeField] private PlayerInteractor[] _playerInteractors;

    [Header("Event Control")]
    [SerializeField] private string _lockEvent = "PlayerControlLocked";
    [SerializeField] private string _unlockEvent = "PlayerControlUnlocked";

    [Header("Options")]
    [SerializeField] private bool _collectTargetsOnInitialize = true;
    [SerializeField] private bool _unlockOnInitialize = true;

    [Header("Debug")]
    [SerializeField] private bool _isLocked;
    [SerializeField] private int _lockCount;
    [SerializeField] private List<string> _lockReasons = new List<string>();

    private readonly HashSet<string> _locks = new HashSet<string>();

    public bool IsLocked => _isLocked;

    public void Initialize()
    {
        Instance = this;

        if (_collectTargetsOnInitialize)
            CollectTargets();

        GameEventBus.OnGameEvent -= OnGameEvent;
        GameEventBus.OnGameEvent += OnGameEvent;

        if (_unlockOnInitialize)
        {
            _locks.Clear();
            SyncDebug();
            ApplyLockState();
        }
    }

    [ContextMenu("Collect Targets")]
    public void CollectTargets()
    {
        _characters = FindObjectsOfType<FpsCharacter>(true);
        _playerLooks = FindObjectsOfType<PlayerLook>(true);
        _playerInteractors = FindObjectsOfType<PlayerInteractor>(true);
    }

    public void LockPlayer(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            reason = "Unknown";

        _locks.Add(reason);
        SyncDebug();
        ApplyLockState();
    }

    public void UnlockPlayer(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            reason = "Unknown";

        _locks.Remove(reason);
        SyncDebug();
        ApplyLockState();
    }

    public void ClearAllLocks()
    {
        _locks.Clear();
        SyncDebug();
        ApplyLockState();
    }

    private void OnGameEvent(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(_lockEvent) && eventId == _lockEvent)
            LockPlayer(eventId);

        if (!string.IsNullOrWhiteSpace(_unlockEvent) && eventId == _unlockEvent)
            ClearAllLocks();
    }

    private void ApplyLockState()
    {
        _isLocked = _locks.Count > 0;

        for (int i = 0; i < _characters.Length; i++)
            if (_characters[i] != null)
                _characters[i].SetMovementEnabled(!_isLocked);

        for (int i = 0; i < _playerLooks.Length; i++)
            if (_playerLooks[i] != null)
                _playerLooks[i].SetLookEnabled(!_isLocked);

        for (int i = 0; i < _playerInteractors.Length; i++)
            if (_playerInteractors[i] != null)
                _playerInteractors[i].SetInteractionEnabled(!_isLocked);
    }

    private void SyncDebug()
    {
        _lockReasons.Clear();
        foreach (string reason in _locks)
            _lockReasons.Add(reason);

        _lockCount = _locks.Count;
    }

    private void OnDestroy()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
    }
}
