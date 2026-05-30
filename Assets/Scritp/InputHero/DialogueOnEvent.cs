using UnityEngine;

[DisallowMultipleComponent]
public class DialogueOnEvent : MonoBehaviour
{
    [Header("Event")]
    [Tooltip("Событие, после которого запустится диалог. Например: BagOpened.")]
    [SerializeField] private string _eventName = "BagOpened";

    [Header("Dialogue")]
    [SerializeField] private DialogueSequence _sequence;
    [SerializeField] private DialoguePlayMode _playMode = DialoguePlayMode.Queue;

    [Header("Options")]
    [Tooltip("Если включено, реакция сработает только один раз.")]
    [SerializeField] private bool _oneShot = true;

    [Header("Debug")]
    [SerializeField] private bool _isListening;
    [SerializeField] private bool _wasTriggered;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        if (_isListening)
            GameEventBus.OnGameEvent -= OnGameEvent;

        GameEventBus.OnGameEvent += OnGameEvent;
        _isListening = true;
    }

    private void OnDestroy()
    {
        if (!_isListening)
            return;

        GameEventBus.OnGameEvent -= OnGameEvent;
        _isListening = false;
    }

    private void OnGameEvent(string eventId)
    {
        if (eventId != _eventName)
            return;

        if (_oneShot && _wasTriggered)
            return;

        _wasTriggered = true;
        PlayDialogue();
    }

    private void PlayDialogue()
    {
        if (_sequence == null)
            return;

        DialogueService service = DialogueService.Instance;
        if (service == null)
            return;

        switch (_playMode)
        {
            case DialoguePlayMode.PlayNow:
                service.PlayNow(_sequence);
                break;

            case DialoguePlayMode.Queue:
                service.Queue(_sequence);
                break;

            case DialoguePlayMode.IgnoreIfBusy:
                if (!service.IsBusy())
                    service.Play(_sequence);
                break;
        }
    }
}
