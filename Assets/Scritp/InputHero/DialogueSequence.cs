using UnityEngine;

[CreateAssetMenu(menuName = "LIFT/Dialogue Sequence", fileName = "DialogueSequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Id")]
    [Tooltip("ID диалога для удобства. Например: Dialogue_BagOpened.")]
    [SerializeField] private string _dialogueId = "Dialogue_New";

    [Header("Lines")]
    [SerializeField] private DialogueLine[] _lines;

    public string DialogueId => _dialogueId;
    public DialogueLine[] Lines => _lines;
    public int LineCount => _lines == null ? 0 : _lines.Length;

    public DialogueLine GetLine(int index)
    {
        if (_lines == null)
            return null;

        if (index < 0 || index >= _lines.Length)
            return null;

        return _lines[index];
    }
}
