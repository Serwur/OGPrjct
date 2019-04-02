using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue : TimerManager.IOnCountdownEnd
{
    public string dialogueName = "";
    public string dialogue = "";
    public string ownerName = "";
    public string objectName = "";
    public float speed = 0.65f;
    public AudioClip audioClip = null;

    private Entity entity = null;
    private long countdownID;
    private int currentChar = 0;
    private bool hasEnded = false;
    private Text dialogueText = null;

    #region Public Methods
    public void NextChar()
    {
        dialogueText.text += dialogue[currentChar++];
        if (currentChar == dialogue.Length) {
            TimerManager.RemoveCountdown( countdownID );
            hasEnded = true;
        } else {
            // ZMIENIC POTEM NA STAŁĄ PRĘDKOŚĆ
            TimerManager.ResetCountdown( countdownID, 1 / ( 0.9f + speed / 10f ) - 1 );
        }
    }

    public bool Init()
    {
        if (ownerName == null)
            return false;
        Entity entity = GameManager.GetEntityByName( objectName );
        return entity != null;
    }

    public void StartDialogue(DialogueType type)
    {
        // Creates frames
        if (type == DialogueType.CLOUD) {
            dialogueText = DialogueManager.GetCloudFrame( entity );
        } else {
            dialogueText = DialogueManager.GetCinematicFrame();
        }
        dialogueText.text = ownerName + ": ";
        // Start timer if speed isn't 0
        if (speed == 1f) {
            PushToEnd();
        } else {
            countdownID = TimerManager.StartCountdown( 1 / ( 0.9f + speed / 10f ) - 1, true, this );
        }
    }

    public void PushToEnd()
    {
        hasEnded = true;
        dialogueText.text = ownerName + ": " + dialogue;
        TimerManager.RemoveCountdown( countdownID );
    }

    public void OnCountdownEnd(long id)
    {
        if (id == countdownID) {
            NextChar();
        }
    }

    public void _Reset()
    {
        TimerManager.RemoveCountdown( countdownID );
        dialogueText = null;
        hasEnded = false;
        currentChar = 0;
        entity = null;
    }

    public Dialogue Clone()
    {
        Dialogue clone = new Dialogue {
            dialogueName = dialogueName,
            dialogue = dialogue,
            audioClip = audioClip,
            objectName = objectName,
            ownerName = ownerName,
            speed = speed
        };
        return clone;
    }
    #endregion

    #region Setters And Getters
    public string Text { get => dialogue; }
    public float Speed { get => speed; }
    public bool HasEnded { get => hasEnded; }
    #endregion
}