using System;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogueEventRouter : MonoBehaviour
{
    [Serializable]
    private class DialogueEventBinding
    {
        [Header("Event")]
        [Tooltip("Событие, на которое реагирует диалог. Например: BagOpened.")]
        public string EventName;

        [Header("Dialogue")]
        [Tooltip("DialogueSequence, который нужно запустить после события.")]
        public DialogueSequence Sequence;

        [Tooltip("Как запускать диалог: сразу, в очередь или только если сейчас нет другого диалога.")]
        public DialoguePlayMode PlayMode = DialoguePlayMode.Queue;

        [Header("Options")]
        [Tooltip("Если включено, эта связка сработает только один раз.")]
        public bool OneShot = true;

        [Header("Debug")]
        public bool WasTriggered;
    }

    [Header("Bindings")]
    [Tooltip("Список связок: событие -> диалог. Теперь не нужно создавать отдельный объект DialogueOnEvent под каждое событие.")]
    [SerializeField] private DialogueEventBinding[] _bindings;

    [Header("Debug")]
    [SerializeField] private bool _isListening;
    [SerializeField] private string _lastEvent;
    [SerializeField] private string _lastDialogue;

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
        if (string.IsNullOrWhiteSpace(eventId) || _bindings == null)
            return;

        _lastEvent = eventId;

        for (int i = 0; i < _bindings.Length; i++)
        {
            DialogueEventBinding binding = _bindings[i];
            if (binding == null)
                continue;

            if (binding.EventName != eventId)
                continue;

            if (binding.OneShot && binding.WasTriggered)
                continue;

            binding.WasTriggered = true;
            PlayBinding(binding);
        }
    }

    private void PlayBinding(DialogueEventBinding binding)
    {
        if (binding == null || binding.Sequence == null)
            return;

        DialogueService service = DialogueService.Instance;
        if (service == null)
            return;

        _lastDialogue = binding.Sequence.DialogueId;

        switch (binding.PlayMode)
        {
            case DialoguePlayMode.PlayNow:
                service.PlayNow(binding.Sequence);
                break;

            case DialoguePlayMode.Queue:
                service.Queue(binding.Sequence);
                break;

            case DialoguePlayMode.IgnoreIfBusy:
                if (!service.IsBusy())
                    service.Play(binding.Sequence);
                break;
        }
    }
}
