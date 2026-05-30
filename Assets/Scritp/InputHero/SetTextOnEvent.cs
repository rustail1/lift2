using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class SetTextOnEvent : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private string _eventName = "CriticalError";

    [Header("Text Target")]
    [SerializeField] private TMP_Text _textTarget;

    [Header("Text")]
    [TextArea(2, 8)]
    [SerializeField] private string _text = "ERROR";

    [Header("Options")]
    [SerializeField] private bool _clearOnInitialize;
    [SerializeField] private bool _enableTargetOnEvent = true;

    [Header("Debug")]
    [SerializeField] private bool _isInitialized;
    [SerializeField] private int _triggerCount;

    public void Initialize()
    {
        _isInitialized = true;
        GameEventBus.OnGameEvent -= OnGameEvent;
        GameEventBus.OnGameEvent += OnGameEvent;

        if (_clearOnInitialize && _textTarget != null)
            _textTarget.text = string.Empty;
    }

    private void OnGameEvent(string eventId)
    {
        if (eventId != _eventName)
            return;

        Apply();
    }

    [ContextMenu("Apply Now")]
    public void Apply()
    {
        _triggerCount++;

        if (_textTarget == null)
            return;

        if (_enableTargetOnEvent)
            _textTarget.gameObject.SetActive(true);

        _textTarget.text = _text;
    }

    private void OnDestroy()
    {
        GameEventBus.OnGameEvent -= OnGameEvent;
    }
}
