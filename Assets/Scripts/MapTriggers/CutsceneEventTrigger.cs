using Dialogue;
using UnityEngine;

namespace MapTriggers
{
    public class CutsceneEventTrigger : MonoBehaviour
    {
        public DialogueScriptable dialogueData;

        //This event simply takes a dialoguescriptable, places it into the dialogue reader and starts the dialogue.

        public void TriggerEvent()
        {
            DialogueReader reader = FindObjectOfType<DialogueReader>();
            reader.dialogueData = dialogueData;
            reader.StartDialogue();
        }
    }
}
