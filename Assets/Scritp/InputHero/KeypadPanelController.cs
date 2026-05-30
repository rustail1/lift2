using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class KeypadPanelController : MonoBehaviour
{
    [Header("Code")]
    [SerializeField] private string _expectedCode = "1407";
    [SerializeField] private int _maxLength = 8;
    [SerializeField] private bool _clearAfterSuccess = true;
    [SerializeField] private bool _clearAfterFail = true;

    [Header("Conditions")]
    [SerializeField] private string _requiredItem;
    [SerializeField] private string _requiredEvent;
    [SerializeField] private string _failText = "Панель сейчас недоступна.";

    [Header("Output Events")]
    [SerializeField] private string _successEvent = "CodeAccepted";
    [SerializeField] private string _failEvent = "CodeDenied";
    [SerializeField] private bool _saveEventsToScenarioState = true;

    [Header("UI")]
    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private string _emptyDisplay = "----";
    [SerializeField] private string _successDisplay = "OK";
    [SerializeField] private string _failDisplay = "ERROR";
    [SerializeField] private bool _maskInput;
    [SerializeField] private char _maskCharacter = '*';

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _buttonClip;
    [SerializeField] private AudioClip _successClip;
    [SerializeField] private AudioClip _failClip;

    [Header("Debug")]
    [SerializeField] private string _currentInput;
    [SerializeField] private bool _lastResultSuccess;

    public string CurrentInput => _currentInput;

    public void Initialize()
    {
        _currentInput = string.Empty;
        _lastResultSuccess = false;
        UpdateDisplay();
    }

    public bool CanUse()
    {
        if (!string.IsNullOrWhiteSpace(_requiredItem))
        {
            InventoryService inventory = InventoryService.Instance;
            if (inventory == null || !inventory.HasItem(_requiredItem))
                return false;
        }

        if (!string.IsNullOrWhiteSpace(_requiredEvent))
        {
            ScenarioStateService scenario = ScenarioStateService.Instance;
            if (scenario == null || !scenario.HasEvent(_requiredEvent))
                return false;
        }

        return true;
    }

    public string GetFailText()
    {
        return _failText;
    }

    public void PressDigit(string digit)
    {
        if (!CanUse())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            return;
        }

        if (string.IsNullOrEmpty(digit))
            return;

        PlayClip(_buttonClip);

        if (_currentInput.Length >= _maxLength)
            return;

        _currentInput += digit;
        UpdateDisplay();
    }

    public void Backspace()
    {
        if (!CanUse())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            return;
        }

        PlayClip(_buttonClip);

        if (string.IsNullOrEmpty(_currentInput))
            return;

        _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
        UpdateDisplay();
    }

    public void ClearInput()
    {
        if (!CanUse())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            return;
        }

        PlayClip(_buttonClip);
        _currentInput = string.Empty;
        UpdateDisplay();
    }

    public void Submit()
    {
        if (!CanUse())
        {
            InteractionUIService.Instance?.ShowHint(_failText);
            return;
        }

        bool success = _currentInput == _expectedCode;
        _lastResultSuccess = success;

        if (success)
        {
            SetDisplayText(_successDisplay);
            PlayClip(_successClip);
            FireEvent(_successEvent);

            if (_clearAfterSuccess)
                _currentInput = string.Empty;
        }
        else
        {
            SetDisplayText(_failDisplay);
            PlayClip(_failClip);
            FireEvent(_failEvent);

            if (_clearAfterFail)
                _currentInput = string.Empty;
        }
    }

    private void UpdateDisplay()
    {
        if (string.IsNullOrEmpty(_currentInput))
        {
            SetDisplayText(_emptyDisplay);
            return;
        }

        if (!_maskInput)
        {
            SetDisplayText(_currentInput);
            return;
        }

        SetDisplayText(new string(_maskCharacter, _currentInput.Length));
    }

    private void SetDisplayText(string value)
    {
        if (_displayText != null)
            _displayText.text = value;
    }

    private void FireEvent(string eventName)
    {
        if (string.IsNullOrWhiteSpace(eventName))
            return;

        if (_saveEventsToScenarioState && ScenarioStateService.Instance != null)
            ScenarioStateService.Instance.TriggerEvent(eventName);
        else
            GameEventBus.Trigger(eventName);
    }

    private void PlayClip(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
            _audioSource.PlayOneShot(clip);
    }
}
