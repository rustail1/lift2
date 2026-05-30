using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class SubtitleUIService : MonoBehaviour
{
    public static SubtitleUIService Instance { get; private set; }

    [Header("Root")]
    [Tooltip("Общий объект панели субтитров. Можно оставить пустым, тогда будут включаться только тексты.")]
    [SerializeField] private GameObject _subtitleRoot;

    [Header("TextMeshPro")]
    [Tooltip("TMP текст имени говорящего.")]
    [SerializeField] private TMP_Text _speakerText;

    [Tooltip("TMP текст самой реплики.")]
    [SerializeField] private TMP_Text _lineText;

    [Header("Options")]
    [Tooltip("Если включено, имя говорящего добавляется в основной текст, если отдельное поле Speaker Text не назначено.")]
    [SerializeField] private bool _prependSpeakerIfNoSpeakerField = true;

    [Header("Typewriter")]
    [Tooltip("Если включено, текст реплики появляется постепенно, как будто его печатают/говорят.")]
    [SerializeField] private bool _useTypewriter = true;

    [Tooltip("Скорость печати символов в секунду.")]
    [SerializeField] private float _charactersPerSecond = 35f;

    [Tooltip("Сколько символов показать сразу в начале реплики. 0 = печатать с пустого текста.")]
    [SerializeField] private int _startVisibleCharacters = 0;

    [Header("Debug")]
    [SerializeField] private bool _isVisible;
    [SerializeField] private bool _isTyping;
    [SerializeField] private string _currentSpeaker;
    [SerializeField] private string _currentLine;
    [SerializeField] private string _fullLineForDisplay;
    [SerializeField] private int _visibleCharacters;
    [SerializeField] private int _targetCharacters;

    private float _typewriterProgress;

    public bool IsVisible => _isVisible;
    public bool IsTyping => _isTyping;
    public float CharactersPerSecond => Mathf.Max(1f, _charactersPerSecond);

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        Instance = this;
        HideSubtitle();
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick(float deltaTime)
    {
        if (!_isVisible || !_isTyping || _lineText == null)
            return;

        _typewriterProgress += CharactersPerSecond * deltaTime;
        _visibleCharacters = Mathf.Clamp(Mathf.FloorToInt(_typewriterProgress), 0, _targetCharacters);
        _lineText.maxVisibleCharacters = _visibleCharacters;

        if (_visibleCharacters >= _targetCharacters)
        {
            _isTyping = false;
            _lineText.maxVisibleCharacters = int.MaxValue;
        }
    }

    public void ShowSubtitle(string speaker, string text)
    {
        _isVisible = true;
        _currentSpeaker = speaker;
        _currentLine = text;

        if (_subtitleRoot != null)
            _subtitleRoot.SetActive(true);

        if (_speakerText != null)
        {
            _speakerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(speaker));
            _speakerText.text = speaker;
        }

        if (_lineText != null)
        {
            _lineText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));

            if (_speakerText == null && _prependSpeakerIfNoSpeakerField && !string.IsNullOrWhiteSpace(speaker))
                _fullLineForDisplay = speaker + ": " + text;
            else
                _fullLineForDisplay = text;

            _lineText.text = _fullLineForDisplay;

            if (_useTypewriter && !string.IsNullOrWhiteSpace(_fullLineForDisplay))
                StartTypewriter();
            else
                ShowFullLineInstantly();
        }
    }

    public void HideSubtitle()
    {
        _isVisible = false;
        _isTyping = false;
        _currentSpeaker = string.Empty;
        _currentLine = string.Empty;
        _fullLineForDisplay = string.Empty;
        _visibleCharacters = 0;
        _targetCharacters = 0;
        _typewriterProgress = 0f;

        if (_subtitleRoot != null)
            _subtitleRoot.SetActive(false);

        if (_speakerText != null)
        {
            _speakerText.text = string.Empty;
            _speakerText.gameObject.SetActive(false);
        }

        if (_lineText != null)
        {
            _lineText.text = string.Empty;
            _lineText.maxVisibleCharacters = int.MaxValue;
            _lineText.gameObject.SetActive(false);
        }
    }

    public float GetEstimatedTypewriterDuration(string speaker, string text)
    {
        if (!_useTypewriter)
            return 0f;

        string displayText = text;

        if (_speakerText == null && _prependSpeakerIfNoSpeakerField && !string.IsNullOrWhiteSpace(speaker))
            displayText = speaker + ": " + text;

        int characterCount = string.IsNullOrEmpty(displayText) ? 0 : displayText.Length;
        return characterCount / CharactersPerSecond;
    }

    private void StartTypewriter()
    {
        _lineText.ForceMeshUpdate();

        _targetCharacters = _lineText.textInfo != null
            ? _lineText.textInfo.characterCount
            : _fullLineForDisplay.Length;

        _visibleCharacters = Mathf.Clamp(_startVisibleCharacters, 0, _targetCharacters);
        _typewriterProgress = _visibleCharacters;
        _isTyping = _visibleCharacters < _targetCharacters;
        _lineText.maxVisibleCharacters = _visibleCharacters;

        if (!_isTyping)
            _lineText.maxVisibleCharacters = int.MaxValue;
    }

    private void ShowFullLineInstantly()
    {
        _isTyping = false;
        _targetCharacters = string.IsNullOrEmpty(_fullLineForDisplay) ? 0 : _fullLineForDisplay.Length;
        _visibleCharacters = _targetCharacters;
        _typewriterProgress = _visibleCharacters;

        if (_lineText != null)
            _lineText.maxVisibleCharacters = int.MaxValue;
    }
}
