using UnityEngine;

[DisallowMultipleComponent]
public class PlaySoundOnEvent : MonoBehaviour
{
    [Header("Event")]
    [Tooltip("Событие, после которого нужно проиграть звук. Например: BagOpened.")]
    [SerializeField] private string _eventName = "BagOpened";

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _clip;
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;
    [SerializeField] private bool _useOneShot = true;

    [Header("Options")]
    [Tooltip("Если включено, реакция сработает только один раз.")]
    [SerializeField] private bool _oneShot = false;

    [Header("Debug")]
    [SerializeField] private bool _isListening;
    [SerializeField] private bool _wasTriggered;

    private void Reset()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();
    }

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

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
        Play();
    }

    private void Play()
    {
        if (_audioSource == null)
            return;

        if (_clip != null)
        {
            if (_useOneShot)
                _audioSource.PlayOneShot(_clip, _volume);
            else
            {
                _audioSource.clip = _clip;
                _audioSource.volume = _volume;
                _audioSource.Play();
            }

            return;
        }

        _audioSource.Play();
    }
}
