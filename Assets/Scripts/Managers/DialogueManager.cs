using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager Instance;
    private static GameObject CloudFrame;
    private static GameObject CinematicFrame;
    private static Text CloudText;
    private static Text CinematicText;

    private DialogueList currentDialogueList;

    #region Unity API
    private void Awake()
    {
        if (Instance != null) {
            Debug.LogError( "DialogueManager::Awake::(Trying to create more than one dialogue manager!)" );
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // CloudFrame = GameObject.Find( "CloudFrame_Canvas" );
        //CloudText = GameObject.Find( "Cloud_Text" ).GetComponent<Text>();
        //CloudFrame.gameObject.SetActive( false );
        CinematicFrame = GameObject.Find( "Cinematic_Canvas" );
        CinematicText = GameObject.Find( "Cinematic_Text" ).GetComponent<Text>();
        CinematicFrame.gameObject.SetActive( false );
    }

    #endregion

    #region Public Methods
    public static void StartDialogues(DialogueList dialogueList)
    {
        GameManager.PauseEntities( true );
        Instance.currentDialogueList = dialogueList;
        dialogueList.NextDialog();
    }

    public static void _Reset()
    {
        if (Instance.currentDialogueList != null) {
            Instance.currentDialogueList._Reset();
            Instance.currentDialogueList = null;
        }
        // CloudFrame.SetActive( false );
        // CloudFrame.transform.parent = null;
        CinematicFrame.SetActive( false );
        GameManager.PauseEntities( false );
    }

    public static Text GetCloudFrame(Entity entity)
    {
        CloudFrame.SetActive( true );
        CloudText.text = "";
        CloudFrame.transform.parent = entity.transform;
        CloudFrame.transform.localPosition = Vector3.zero;
        return CloudText;
    }

    public static Text GetCinematicFrame()
    {
        CinematicFrame.SetActive( true );
        CinematicText.text = "";
        return CinematicText;
    }

    public static void PushNextDialogue()
    {
        if (Instance.currentDialogueList != null) {
            if (Instance.currentDialogueList.CurrentDialogue.HasEnded) {
                Instance.currentDialogueList.NextDialog();
            } else {
                Instance.currentDialogueList.CurrentDialogue.PushToEnd();
            }
        }
    }

    // DEV METHOD TO DELETE IN FUTURE
    public static bool HasEndedDialogueList()
    {
        if (Instance.currentDialogueList == null)
            return true;
        return Instance.currentDialogueList.HasEndedList;
    }
    #endregion
}
