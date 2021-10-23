using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem
{
    [System.Serializable]
    public abstract class InterjectBase : ScriptableObject
    {
        public string stateName;
        public string stateId;
        public string prevStateId;
        public bool initialized = false;
        public List<DialogueNode> dialogueNodes = new List<DialogueNode>();

        [HideInInspector] public BattleSystemCore battleCore;
        [HideInInspector] public BattleSystemStateManager parent;

        public void StatePreamble(BattleSystemCore core, BattleSystemStateManager manager)
        {
            this.battleCore = core;
            this.parent = manager;
            //Perhaps other stuff needs to be prepped here.
        }

        public abstract void Init(); // Allows the state to initialize and connect to UI and so on.
        public abstract void UpdateState(out bool endOfLife);
        public abstract void Disconnect(); // De-couples the state from ui and so on, the last thing we do.

        public virtual void OnSubmitButton(InputAction.CallbackContext obj)
        {
            //Do nothing as the default
        }

        public virtual void OnCancelButton(InputAction.CallbackContext obj)
        {
            //Do nothing as the default
        }

        public virtual void OnMoveAxis(InputAction.CallbackContext obj)
        {
            //Do nothing as the default
        }
    }

    [System.Serializable]
    public class DialogueNode
    {
        public Sprite speakerPortrait;
        public Color backgroundColor;
        public string text;
        public float buildDelay;
    }
}
