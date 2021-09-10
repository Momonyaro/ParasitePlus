using Dialogue.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class CharNameBoxComponent : DialogueComponent
    {
        private CharacterNameBox characterNameBox;
        private bool finishedCreation = false;
        
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            componentPrefab = objectPrefab;
            finishedCreation = false;
        }

        public override void Update(out bool endOfLife)
        {
            endOfLife = finishedCreation;
            
            if (characterNameBox == null)
            {
                if (IsNull()) return;
                characterNameBox = GetCurrentInstance().GetComponent<CharacterNameBox>();
            }
        }

        public override void OnSubmitInput(InputAction.CallbackContext context)
        {
            //Call to write the selected character to the playerName
            if (characterNameBox == null) return;
            
            characterNameBox.AddSelectedToPlayerName();
        }

        public override void OnCancelInput(InputAction.CallbackContext context)
        {
            //Call to delete the last character of the playerName
            if (characterNameBox == null) return;
            
            characterNameBox.RemoveLastFromPlayerName();
        }

        public override void OnStartInput(InputAction.CallbackContext context)
        {
            //Call to delete the last character of the playerName
            if (characterNameBox == null) return;
            
            Debug.Log("Verifying player name");
            characterNameBox.VerifyNameAndCreateSlim();
            finishedCreation = true;
        }

        public override void OnMoveInput(InputAction.CallbackContext context)
        {
            //Call to move the cursor of the keyboard
            if (characterNameBox == null) return;

            Vector2 delta = context.ReadValue<Vector2>();
            Vector2Int intDelta = new Vector2Int(Mathf.RoundToInt(delta.x), Mathf.RoundToInt(delta.y));
            
            characterNameBox.MoveCursor(intDelta.x, intDelta.y);
        }

        public override ComponentTypes GetComponentType()
        {
            return ComponentTypes.PLAYER_NAME_BOX;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }
    }
}