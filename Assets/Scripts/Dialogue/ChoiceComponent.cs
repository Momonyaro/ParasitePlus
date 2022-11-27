using Dialogue.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class ChoiceComponent : DialogueComponent
    {
        public bool finished = false;
        private ChoiceReceiver choiceReceiver;
        public List<ChoiceData> choices = new List<ChoiceData>();

        private DialogueScriptable dialogueScriptable;

        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            SetNewInstance(null); // Nullify instance so that we don't have any ghosts spooking the code.
            dialogueScriptable = parent;

            componentPrefab = objectPrefab;

            componentPrefab.GetComponent<ChoiceReceiver>().CreateList(choices.ToArray());
            finished = false;
        }

        public override void Update(out bool endOfLife)
        {
            endOfLife = finished;

            if (choiceReceiver == null)
            {
                if (IsNull()) return;
                choiceReceiver = GetCurrentInstance().GetComponent<ChoiceReceiver>();
            }
        }

        public override void OnSubmitInput(InputAction.CallbackContext context)
        {
        }

        public override void OnCancelInput(InputAction.CallbackContext context)
        {
            // We're not using this
        }

        public override void OnStartInput(InputAction.CallbackContext context)
        {
            dialogueScriptable.transitionToOnEof = choices[choiceReceiver.currentIndex].dialogueBranch;
            dialogueScriptable.wipeOldDataOnLoad = false;
            finished = true;
        }

        public override void OnMoveInput(InputAction.CallbackContext context)
        {
            //Call to move the cursor of the keyboard
            if (choiceReceiver == null) return;

            Vector2 delta = context.ReadValue<Vector2>();
            Vector2Int intDelta = new Vector2Int(Mathf.RoundToInt(delta.x), Mathf.RoundToInt(delta.y));

            choiceReceiver.MoveCursor(intDelta.y);
        }

        public override ComponentTypes GetComponentType()
        {
            return ComponentTypes.CHOICE;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }

        [System.Serializable]
        public struct ChoiceData
        {
            public string text;
            public DialogueScriptable dialogueBranch;
        }
    }
}
