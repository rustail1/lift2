using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InteractionUIService : MonoBehaviour
{
    public static InteractionUIService Instance { get; private set; }

    [Header("Crosshair")]
    [Tooltip("Обычный прицел. Активен, когда игрок ни на что интерактивное не смотрит.")]
    [SerializeField] private GameObject _normalCrosshair;

    [Tooltip("Активный прицел. Активен, когда игрок смотрит на интерактивный объект.")]
    [SerializeField] private GameObject _activeCrosshair;

    [Header("Text")]
    [Tooltip("Текст подсказки. Используется TextMeshProUGUI / TMP_Text.")]
    [SerializeField] private TMP_Text _interactionText;

    [Tooltip("Текст коротких ошибок: нет предмета, рано брать предмет и т.п. Используется TextMeshProUGUI / TMP_Text.")]
    [SerializeField] private TMP_Text _hintText;

    [Header("Progress")]
    [Tooltip("Image с Fill Amount для Hold-взаимодействия.")]
    [SerializeField] private Image _holdProgressFill;

    [Tooltip("Image с Fill Amount для Mash-взаимодействия.")]
    [SerializeField] private Image _mashProgressFill;

    [Header("Hint")]
    [SerializeField] private float _hintDuration = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool _hasTarget;
    [SerializeField] private string _currentText;
    [SerializeField] private string _currentHint;

    private float _hintTimer;

    // Вызывается из LiftGameInitializer. Awake специально не используется.
    public void Initialize()
    {
        Instance = this;
        SetInteractionTarget(false, string.Empty);
        SetHoldProgress(0f);
        SetMashProgress(0f);
        HideHintNow();
    }

    // Вызывается из LiftGameUpdateRunner. Update специально не используется.
    public void Tick(float deltaTime)
    {
        if (_hintTimer <= 0f)
            return;

        _hintTimer -= deltaTime;

        if (_hintTimer <= 0f)
            HideHintNow();
    }

    public void SetInteractionTarget(bool active, string text)
    {
        _hasTarget = active;
        _currentText = active ? text : string.Empty;

        if (_normalCrosshair != null)
            _normalCrosshair.SetActive(!active);

        if (_activeCrosshair != null)
            _activeCrosshair.SetActive(active);

        if (_interactionText != null)
        {
            _interactionText.gameObject.SetActive(active && !string.IsNullOrWhiteSpace(text));
            _interactionText.text = _currentText;
        }
    }

    public void ShowHint(string text)
    {
        _currentHint = text;
        _hintTimer = _hintDuration;

        if (_hintText == null)
            return;

        _hintText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        _hintText.text = text;
    }

    public void SetHoldProgress(float value)
    {
        SetFill(_holdProgressFill, value);
    }

    public void SetMashProgress(float value)
    {
        SetFill(_mashProgressFill, value);
    }

    private void HideHintNow()
    {
        _currentHint = string.Empty;
        _hintTimer = 0f;

        if (_hintText != null)
        {
            _hintText.text = string.Empty;
            _hintText.gameObject.SetActive(false);
        }
    }

    private void SetFill(Image image, float value)
    {
        if (image == null)
            return;

        float clamped = Mathf.Clamp01(value);
        image.fillAmount = clamped;
        image.gameObject.SetActive(clamped > 0.001f);
    }
}
