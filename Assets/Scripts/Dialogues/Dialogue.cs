using UnityEngine;
using UnityEngine.UI;
using ColdCry.Utility;
using ColdCry.Core;
using ColdCry.Objects;

namespace ColdCry
{
    [System.Serializable]
    public class Dialogue : IOnCountdownEnd
    {
        public string dialogueName = "";
        public string dialogue = "";
        public string ownerName = "";
        public string objectName = "";
        public float speed = 0.65f;
        public AudioClip audioClip = null;
        public bool followObject = false;
        public float followSpeed = 0.45f;

        private Entity entity = null;
        private long countdownID;
        private int currentChar = 0;
        private bool hasEnded = false;
        private Text dialogueText = null;
        private bool failed = false;

        #region Public Methods
        /// <summary>
        /// Inits the dialogue by setting the proper entity on scene
        /// </summary>
        /// <returns>TRUE if entity by given <b>object name</b> exists, otherwise FALSE</returns>
        public bool Init()
        {
            if (ownerName == null) {
                failed = true;
                return false;
            }
            entity = GameManager.GetEntityByName( objectName );
            failed = entity == null;
            return !failed;
        }

        /// <summary>
        /// Appears next char from <b>dialogue</b> field on the screen
        /// </summary>
        public void NextChar()
        {
            dialogueText.text += dialogue[currentChar++];
            if (currentChar == dialogue.Length) {
                TimerManager.Destroy( countdownID );
                hasEnded = true;
            } else {
                // ZMIENIC POTEM NA STAŁĄ PRĘDKOŚĆ
                TimerManager.Restart( countdownID, 1 / ( 0.9f + speed / 10f ) - 1 );
            }
        }

        /// <summary>
        /// Starts the dialogue
        /// </summary>
        /// <param name="type">Type of dialogue, where should appear on screen</param>
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
                countdownID = TimerManager.Start( 1 / ( 0.9f + speed / 10f ) - 1, this );
            }
            if (followObject) {
                CameraManager.FollowTarget( entity.transform, followSpeed );
            }
        }

        /// <summary>
        /// Ends the dialogue appearing by putting whole dialogue string on screen
        /// </summary>
        public void PushToEnd()
        {
            hasEnded = true;
            dialogueText.text = ownerName + ": " + dialogue;
            TimerManager.Destroy( countdownID );
        }


        public void OnCountdownEnd(long id, float overtime)
        {
            if (id == countdownID) {
                NextChar();
            }
        }

        /// <summary>
        /// Resets dialogue fields
        /// </summary>
        public void _Reset()
        {
            TimerManager.Destroy( countdownID );
            dialogueText = null;
            hasEnded = false;
            currentChar = 0;
            entity = null;
            failed = false;
        }

        /// <summary>
        /// Clones the dialogue
        /// </summary>
        /// <returns>New Dialogue object with copied fields</returns>
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
        public bool Failed { get => failed; }
        #endregion
    }
}