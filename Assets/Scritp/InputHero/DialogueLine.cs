using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    [Tooltip("Кто говорит. Например: Напарник, Лифтёр, Интерком.")]
    [SerializeField] private string _speakerName = "Напарник";

    [Header("Text")]
    [Tooltip("Текст реплики для субтитров.")]
    [TextArea(2, 5)]
    [SerializeField] private string _text;

    [Header("Audio")]
    [Tooltip("Опционально: аудиоклип реплики. Если пусто, будут только субтитры.")]
    [SerializeField] private AudioClip _audioClip;

    [Header("Timing")]
    [Tooltip("Задержка перед этой репликой.")]
    [SerializeField] private float _delayBefore = 0f;

    [Tooltip("Длительность показа субтитра. Если 0 и есть AudioClip, будет использована длина аудио.")]
    [SerializeField] private float _duration = 3f;

    public string SpeakerName => _speakerName;
    public string Text => _text;
    public AudioClip AudioClip => _audioClip;
    public float DelayBefore => Mathf.Max(0f, _delayBefore);

    public float GetDuration()
    {
        if (_duration > 0f)
            return _duration;

        if (_audioClip != null)
            return _audioClip.length;

        return 3f;
    }
}
