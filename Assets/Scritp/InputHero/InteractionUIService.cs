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

    [Tooltip("Если включено, оба прицела скрыты. Используется в inspect mode / крупном плане панели.")]
    [SerializeField] private bool _suppressCrosshair;

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
        _suppressCrosshair = false;
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

        RefreshCrosshair();

        if (_interactionText != null)
        {
            _interactionText.gameObject.SetActive(active && !string.IsNullOrWhiteSpace(text));
            _interactionText.text = _currentText;
        }
    }

    // Используй это для крупного плана панели: true = скрыть оба прицела, false = вернуть обычное поведение.
    public void SetCrosshairSuppressed(bool suppressed)
    {
        _suppressCrosshair = suppressed;
        RefreshCrosshair();
    }

    // Старое удобное API: true = показать, false = скрыть.
    public void SetCrosshairVisible(bool visible)
    {
        SetCrosshairSuppressed(!visible);
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

    private void RefreshCrosshair()
    {
        if (_normalCrosshair != null)
            _normalCrosshair.SetActive(!_suppressCrosshair && !_hasTarget);

        if (_activeCrosshair != null)
            _activeCrosshair.SetActive(!_suppressCrosshair && _hasTarget);
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
