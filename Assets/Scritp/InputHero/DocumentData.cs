using UnityEngine;

[CreateAssetMenu(menuName = "LIFT/Document Data")]
public class DocumentData : ScriptableObject
{
    [Header("Document")]
    [SerializeField] private string _documentId = "Manual_Page_01";
    [SerializeField] private string _title = "Сервис-мануал";

    [TextArea(5, 20)]
    [SerializeField] private string _text;

    [Tooltip("Картинка/скан страницы. Можно оставить пустым, если нужен только текст.")]
    [SerializeField] private Sprite _image;

    [Tooltip("Если включено, картинка будет показана вместе с текстом.")]
    [SerializeField] private bool _showImage = true;

    [Tooltip("Если включено, текст будет показан вместе с картинкой.")]
    [SerializeField] private bool _showText = true;

    public string DocumentId => _documentId;
    public string Title => _title;
    public string Text => _text;
    public Sprite Image => _image;
    public bool ShowImage => _showImage;
    public bool ShowText => _showText;
}
