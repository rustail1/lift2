using UnityEngine;

[DisallowMultipleComponent]
public class KeypadButton : MonoBehaviour, IInteractable
{
    [Header("Button")]
    [SerializeField] private KeypadPanelController _panel;
    [SerializeField] private KeypadButtonType _buttonType = KeypadButtonType.Digit;
    [SerializeField] private string _digit = "1";

    [Header("Interaction")]
    [SerializeField] private string _interactionName = "Нажать";
    [SerializeField] private string _failText = "Панель сейчас недоступна.";

    [Header("Events")]
    public AK.Wwise.Event panel_button;

    [Header("Debug")]
    [SerializeField] private int _pressCount;

    public bool IsInteractionInProgress => false;

    private void Reset()
    {
        if (_panel == null)
            _panel = GetComponentInParent<KeypadPanelController>();
    }

    public void Initialize()
    {
        if (_panel == null)
            _panel = GetComponentInParent<KeypadPanelController>();
    }

    public string GetInteractionText()
    {
        if (_panel != null && !_panel.CanUse())
            return _panel.GetFailText();

        switch (_buttonType)
        {
            case KeypadButtonType.Digit:
                return _interactionName + " " + _digit;
            case KeypadButtonType.Enter:
                return _interactionName + " ENTER";
            case KeypadButtonType.Clear:
                return _interactionName + " CLEAR";
            case KeypadButtonType.Backspace:
                return _interactionName + " BACK";
            default:
                return _interactionName;
        }
    }

    public bool CanInteract()
    {
        return _panel != null && _panel.CanUse();
    }

    public void InteractDown()
    {
        if (_panel == null)
            return;

        if (!_panel.CanUse())
        {
            InteractionUIService.Instance?.ShowHint(_panel.GetFailText());
            return;
        }

        _pressCount++;

        switch (_buttonType)
        {
            case KeypadButtonType.Digit:
                _panel.PressDigit(_digit);
                panel_button.Post(gameObject);
                break;
            case KeypadButtonType.Enter:
                _panel.Submit();
                break;
            case KeypadButtonType.Clear:
                _panel.ClearInput();
                break;
            case KeypadButtonType.Backspace:
                _panel.Backspace();
                break;
        }
    }

    public void InteractUp() { }
    public void InteractionTick(float deltaTime) { }
    public float GetInteractionProgress() => 0f;
    public InteractionType GetInteractionType() => InteractionType.Click;
}
