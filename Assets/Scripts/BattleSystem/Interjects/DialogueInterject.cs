using System.Collections;
using System.Collections.Generic;
using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.Interjects
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "DialogueInterject", menuName = "Interjects/DialogueInterject", order = 1)]
    public class DialogueInterject : InterjectBase
    {
        DialogueScreenUI dialogueScreen;

        public override void Init()
        {
            dialogueScreen = FindObjectOfType<DialogueScreenUI>();

            dialogueScreen.StartDialogue(dialogueNodes);

            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = !dialogueScreen.IsRunning();
        }

        public override void Disconnect()
        {    
            initialized = false;
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            dialogueScreen.UpdateDialogue();
        }
    }

}