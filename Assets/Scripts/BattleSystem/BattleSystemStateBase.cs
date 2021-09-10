using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem
{
    public abstract class BattleSystemStateBase : ScriptableObject
    {
        public string stateName;
        public string stateId;
        public bool initialized = false;
        
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
        
        public abstract void OnSubmitButton(InputAction.CallbackContext obj);
        public abstract void OnCancelButton(InputAction.CallbackContext obj);
        public abstract void OnMoveAxis(InputAction.CallbackContext obj);
    }
}
