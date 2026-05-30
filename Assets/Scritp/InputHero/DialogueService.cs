using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class DialogueService : MonoBehaviour
{
    public static DialogueService Instance { get; private set; }

    private enum DialogueRuntimeState
    {
        Idle,
        DelayBeforeLine,
        PlayingLine
    }

    [Header("UI")]
    [Tooltip("UI-сервис субтитров. Если пусто, будет использован SubtitleUIService.Instance.")]
    [SerializeField] private SubtitleUIService _subtitleUi;

    [Header("Audio")]
    [Tooltip("AudioSource интеркома/голоса. Если пусто, реплики будут только текстом.")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Options")]
    [Tooltip("Если включено, при старте нового диалога текущий диалог будет остановлен. Если выключено — новый диалог добавится в очередь.")]
    [SerializeField] private bool _interruptByDefault = false;

    [Tooltip("Если включено, субтитр скрывается после завершения последовательности.")]
    [SerializeField] private bool _hideSubtitleOnFinish = true;

    [Tooltip("Если включено, реплика не закончится раньше, чем текст успеет напечататься.")]
    [SerializeField] private bool _extendLineDurationForTypewriter = true;

    [Tooltip("Минимальная пауза после завершения печати текста.")]
    [SerializeField] private float _holdAfterTypewriter = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool _isPlaying;
    [SerializeField] private string _currentDialogueId;
    [SerializeField] private int _currentLineIndex = -1;
    [SerializeField] private string _currentSpeaker;
    [SerializeField] private string _currentText;
    [SerializeField] private int _queuedCount;
    [SerializeField] private float _timer;

    private readonly Queue<DialogueSequence> _queue = new Queue<DialogueSequence>();
    private DialogueSequence _currentSequence;
    private DialogueRuntimeState _state = DialogueRuntimeState.Idle;
    private DialogueLine _pendingLine;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        Instance = this;

        if (_subtitleUi == null)
            _subtitleUi = SubtitleUIService.Instance;

        StopDialogue();
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick(float deltaTime)
    {
        _queuedCount = _queue.Count;

        switch (_state)
        {
            case DialogueRuntimeState.Idle:
                TryStartNextFromQueue();
                break;

            case DialogueRuntimeState.DelayBeforeLine:
                TickDelay(deltaTime);
                break;

            case DialogueRuntimeState.PlayingLine:
                TickLine(deltaTime);
                break;
        }
    }

    public void Play(DialogueSequence sequence)
    {
        if (sequence == null)
            return;

        if (_interruptByDefault || !_isPlaying)
        {
            StartSequence(sequence, true);
            return;
        }

        Queue(sequence);
    }

    public void PlayNow(DialogueSequence sequence)
    {
        StartSequence(sequence, true);
    }

    public void Queue(DialogueSequence sequence)
    {
        if (sequence == null)
            return;

        _queue.Enqueue(sequence);
        _queuedCount = _queue.Count;

        if (!_isPlaying && _state == DialogueRuntimeState.Idle)
            TryStartNextFromQueue();
    }

    public bool IsBusy()
    {
        return _isPlaying || _state != DialogueRuntimeState.Idle;
    }

    public void StopDialogue()
    {
        _queue.Clear();
        _queuedCount = 0;
        _currentSequence = null;
        _pendingLine = null;
        _state = DialogueRuntimeState.Idle;
        _isPlaying = false;
        _currentDialogueId = string.Empty;
        _currentLineIndex = -1;
        _currentSpeaker = string.Empty;
        _currentText = string.Empty;
        _timer = 0f;

        if (_audioSource != null)
            _audioSource.Stop();

        if (_subtitleUi != null)
            _subtitleUi.HideSubtitle();
    }

    private void StartSequence(DialogueSequence sequence, bool clearCurrent)
    {
        if (sequence == null)
            return;

        if (clearCurrent)
        {
            _queue.Clear();
            _queuedCount = 0;

            if (_audioSource != null)
                _audioSource.Stop();
        }

        _currentSequence = sequence;
        _currentDialogueId = sequence.DialogueId;
        _currentLineIndex = -1;
        _isPlaying = true;
        StartNextLine();
    }

    private void TryStartNextFromQueue()
    {
        if (_queue.Count <= 0)
            return;

        StartSequence(_queue.Dequeue(), false);
        _queuedCount = _queue.Count;
    }

    private void StartNextLine()
    {
        if (_currentSequence == null)
        {
            FinishSequence();
            return;
        }

        _currentLineIndex++;

        DialogueLine line = _currentSequence.GetLine(_currentLineIndex);
        if (line == null)
        {
            FinishSequence();
            return;
        }

        _pendingLine = line;
        float delay = line.DelayBefore;

        if (delay > 0f)
        {
            _timer = delay;
            _state = DialogueRuntimeState.DelayBeforeLine;
            return;
        }

        StartLine(line);
    }

    private void TickDelay(float deltaTime)
    {
        _timer -= deltaTime;
        if (_timer > 0f)
            return;

        StartLine(_pendingLine);
    }

    private void StartLine(DialogueLine line)
    {
        if (line == null)
        {
            StartNextLine();
            return;
        }

        _pendingLine = null;
        _currentSpeaker = line.SpeakerName;
        _currentText = line.Text;
        _timer = line.GetDuration();
        _state = DialogueRuntimeState.PlayingLine;

        if (_subtitleUi == null)
            _subtitleUi = SubtitleUIService.Instance;

        if (_subtitleUi != null)
        {
            if (_extendLineDurationForTypewriter)
            {
                float typingDuration = _subtitleUi.GetEstimatedTypewriterDuration(_currentSpeaker, _currentText);
                _timer = Mathf.Max(_timer, typingDuration + Mathf.Max(0f, _holdAfterTypewriter));
            }

            _subtitleUi.ShowSubtitle(_currentSpeaker, _currentText);
        }

        if (_audioSource != null)
        {
            _audioSource.Stop();

            if (line.AudioClip != null)
                _audioSource.PlayOneShot(line.AudioClip);
        }
    }

    private void TickLine(float deltaTime)
    {
        _timer -= deltaTime;
        if (_timer > 0f)
            return;

        StartNextLine();
    }

    private void FinishSequence()
    {
        _currentSequence = null;
        _pendingLine = null;
        _currentDialogueId = string.Empty;
        _currentLineIndex = -1;
        _currentSpeaker = string.Empty;
        _currentText = string.Empty;
        _timer = 0f;
        _isPlaying = false;
        _state = DialogueRuntimeState.Idle;

        if (_hideSubtitleOnFinish && _subtitleUi != null)
            _subtitleUi.HideSubtitle();

        TryStartNextFromQueue();
    }
}
