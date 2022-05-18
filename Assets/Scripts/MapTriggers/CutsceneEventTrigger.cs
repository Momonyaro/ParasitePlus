using Dialogue;
using UnityEngine;
using UnityEngine.Events;

namespace MapTriggers
{
    public class CutsceneEventTrigger : MonoBehaviour
    {
        public DialogueScriptable dialogueData;
        public UnityEvent onDialogueFinished = new UnityEvent();
        private bool watchDialogueStatus = false;
        private DialogueReader reader;

        //This event simply takes a dialoguescriptable, places it into the dialogue reader and starts the dialogue.

        private void Update()
        {
            if (watchDialogueStatus)
            {
                if (!reader.IsRunning)
                {
                    onDialogueFinished?.Invoke();
                    watchDialogueStatus = false;
                }
            }
        }

        public void TriggerEvent()
        {
            reader = FindObjectOfType<DialogueReader>();
            reader.dialogueData = dialogueData;
            reader.StartDialogue();
            watchDialogueStatus = true;
        }
    }
}
