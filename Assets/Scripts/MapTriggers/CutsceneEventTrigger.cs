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
        public bool generateNewGUID = false;
        public string eventKey = System.Guid.NewGuid().ToString();
        private DialogueReader reader;
        bool triggered = false;
        CORE.MapManager mapManager;

        //This event simply takes a dialoguescriptable, places it into the dialogue reader and starts the dialogue.

        private void OnValidate()
        {
            if (generateNewGUID)
            {
                eventKey = System.Guid.NewGuid().ToString();
                generateNewGUID = false;
            }
        }

        private void Awake()
        {
            mapManager = FindObjectOfType<CORE.MapManager>();
        }

        private void Start()
        {
            if (!storePersistant) return;

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
                        mapManager.WritePersistantState(eventKey, true);
                    onDialogueFinished?.Invoke();
                    watchDialogueStatus = false;
                }
            }
        }

        public void TriggerEvent()
        {
            if (triggered) return; 
            if (mapManager.GetPersistantState(eventKey))
            {
                triggered = true;
                onDialogueFinished?.Invoke();
                return;
            }

            mapManager = FindObjectOfType<CORE.MapManager>();

            reader = FindObjectOfType<DialogueReader>();
            reader.dialogueData = dialogueData;
            reader.StartDialogue();
            watchDialogueStatus = true;
        }
    }
}
