using System.Collections.Generic;
using UnityEngine;

public class DialogueList : ScriptableObject
{
    public List<Dialogue> dialogues;
    public DialogueType type = DialogueType.CINEMATIC;

    private Dialogue currentDialogue = null;
    private int currentDialogueIndex = -1;

    public void NextDialog()
    {
        currentDialogueIndex++;
        if (currentDialogueIndex >= dialogues.Count) {
            DialogueManager._Reset();
            return;
        }
        if (currentDialogue != null) {
            currentDialogue._Reset();
        }
        currentDialogue = dialogues[currentDialogueIndex];
        if (!currentDialogue.Init()) {
            NextDialog();
        } else {
            currentDialogue.StartDialogue( this );
        }
    }

    public void _Reset()
    {
        currentDialogue = null;
        currentDialogueIndex = -1;
        foreach (Dialogue dialogue in dialogues)
            dialogue._Reset();
    }

    public override string ToString()
    {
        string basic = base.ToString();
        return basic.Substring( 0, basic.LastIndexOf( '(' ) - 1 );
    }

    public bool HasEndedList { get => currentDialogueIndex == dialogues.Count; }
    public Dialogue CurrentDialog { get => currentDialogue; }
}
