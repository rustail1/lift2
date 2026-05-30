using System;

public static class GameEventBus
{
    public static event Action<string> OnGameEvent;

    public static void Trigger(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        OnGameEvent?.Invoke(eventId);
    }
}
