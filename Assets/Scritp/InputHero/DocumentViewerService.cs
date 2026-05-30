using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DocumentViewerService : MonoBehaviour
{
    public static DocumentViewerService Instance { get; private set; }

    [Header("UI Root")]
    [Tooltip("Общий объект окна документа. Он будет включаться при открытии и выключаться при закрытии.")]
    [SerializeField] private GameObject _documentRoot;

    [Header("UI Fields")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Image _image;

    [Header("Close Input")]
    [SerializeField] private bool _closeOnEscape = true;
    [SerializeField] private bool _closeOnLeftMouse = true;
    [SerializeField] private float _closeInputDelay = 0.15f;

    [Header("Player Lock")]
    [SerializeField] private bool _lockPlayerWhileOpen = true;

    [Header("Events")]
    [SerializeField] private string _documentOpenedEvent = "DocumentOpened";
    [SerializeField] private string _documentClosedEvent = "DocumentClosed";
    [SerializeField] private bool _saveEventsToScenarioState = true;

    [Header("Debug")]
    [SerializeField] private bool _isOpen;
    [SerializeField] private string _currentDocumentId;
    [SerializeField] private string _currentTitle;

    private float _closeTimer;

    public bool IsOpen => _isOpen;

    public void Initialize()
    {
        Instance = this;
        CloseDocument(false);
    }

    public void Tick(float deltaTime)
    {
        if (!_isOpen)
            return;

        if (_closeTimer > 0f)
        {
            _closeTimer -= deltaTime;
            return;
        }

        bool closeRequested = false;

        // В проекте используется New Input System, поэтому нельзя читать UnityEngine.Input.
        // Закрытие документа проверяем через Keyboard.current / Mouse.current.
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (_closeOnEscape && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            closeRequested = true;

        if (_closeOnLeftMouse && mouse != null && mouse.leftButton.wasPressedThisFrame)
            closeRequested = true;

        if (closeRequested)
            CloseDocument(true);
    }

    public void OpenDocument(DocumentData document)
    {
        if (document == null)
            return;

        _isOpen = true;
        _closeTimer = Mathf.Max(0f, _closeInputDelay);
        _currentDocumentId = document.DocumentId;
        _currentTitle = document.Title;

        if (_documentRoot != null)
            _documentRoot.SetActive(true);

        if (_titleText != null)
        {
            _titleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(document.Title));
            _titleText.text = document.Title;
        }

        if (_bodyText != null)
        {
            bool showText = document.ShowText && !string.IsNullOrWhiteSpace(document.Text);
            _bodyText.gameObject.SetActive(showText);
            _bodyText.text = showText ? document.Text : string.Empty;
        }

        if (_image != null)
        {
            bool showImage = document.ShowImage && document.Image != null;
            _image.gameObject.SetActive(showImage);
            _image.sprite = showImage ? document.Image : null;
        }

        if (_lockPlayerWhileOpen)
            PlayerControlLockService.Instance?.LockPlayer("DocumentViewer");

        FireEvent(_documentOpenedEvent);
    }

    public void CloseDocument(bool fireEvent)
    {
        _isOpen = false;
        _closeTimer = 0f;
        _currentDocumentId = string.Empty;
        _currentTitle = string.Empty;

        if (_documentRoot != null)
            _documentRoot.SetActive(false);

        if (_titleText != null)
        {
            _titleText.text = string.Empty;
            _titleText.gameObject.SetActive(false);
        }

        if (_bodyText != null)
        {
            _bodyText.text = string.Empty;
            _bodyText.gameObject.SetActive(false);
        }

        if (_image != null)
        {
            _image.sprite = null;
            _image.gameObject.SetActive(false);
        }

        if (_lockPlayerWhileOpen)
            PlayerControlLockService.Instance?.UnlockPlayer("DocumentViewer");

        if (fireEvent)
            FireEvent(_documentClosedEvent);
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
}
