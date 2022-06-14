using Dialogue;
using UnityEngine;
using UnityEngine.Events;

namespace MapTriggers
{
    public class CutsceneEventTrigger : MonoBehaviour
    {
        public DialogueScriptable dialogueData;
        public bool storePersistant = false;
        public UnityEvent onDialogueFinished = new UnityEvent();
        private bool watchDialogueStatus = false;
        public string eventKey = System.Guid.NewGuid().ToString();
        private DialogueReader reader;
        bool triggered = false;
        CORE.MapManager mapManager;

        //This event simply takes a dialoguescriptable, places it into the dialogue reader and starts the dialogue.

        private void Start()
        {
            if (!storePersistant) return;

            mapManager = FindObjectOfType<CORE.MapManager>();


            if (mapManager.GetPersistantState(eventKey))
            {
                triggered = true;
                onDialogueFinished?.Invoke();
                return;
            }
        }

        private void Update()
        {
            if (watchDialogueStatus)
            {
                if (!reader.IsRunning)
                {
                    if (storePersistant)
                        mapManager.WritePersistantData(eventKey, true);
                    onDialogueFinished?.Invoke();
                    watchDialogueStatus = false;
                }
            }
        }

        public void TriggerEvent()
        {
            if (triggered) return;

            reader = FindObjectOfType<DialogueReader>();
            reader.dialogueData = dialogueData;
            reader.StartDialogue();
            watchDialogueStatus = true;
        }
    }
}
