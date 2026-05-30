using UnityEngine;

public enum PlayerControlLockAction
{
    Lock,
    Unlock,
    ClearAllLocks
}

[DisallowMultipleComponent]
public class PlayerControlLockOnEvent : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private string _eventName = "PlayerControlLocked";

    [Header("Action")]
    [SerializeField] private PlayerControlLockAction _action = PlayerControlLockAction.Lock;
    [SerializeField] private string _reason = "EventLock";

    [Header("Debug")]
    [SerializeField] private int _triggerCount;

    public void Initialize()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
        GameEventBus.OnGameEvent += OnGameEvent;
    }

    private void OnGameEvent(string eventId)
    {
        if (eventId != _eventName)
            return;

        _triggerCount++;

        PlayerControlLockService service = PlayerControlLockService.Instance;
        if (service == null)
            return;

        switch (_action)
        {
            case PlayerControlLockAction.Lock:
                service.LockPlayer(_reason);
                break;

            case PlayerControlLockAction.Unlock:
                service.UnlockPlayer(_reason);
                break;

            case PlayerControlLockAction.ClearAllLocks:
                service.ClearAllLocks();
                break;
        }
    }

    private void OnDestroy()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
    }
}
