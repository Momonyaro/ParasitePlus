using System.Collections.Generic;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

namespace BattleSystem
{
    [System.Serializable]
    public class BattleSystemStateManager
    {
        
        [SerializeReference] public List<BattleSystemStateBase> systemStates = new List<BattleSystemStateBase>();
        [Space()]
        public int currentIndex = 0;

        public AbilityScriptable lastAbility;
        public List<int> targetedEntities = new List<int>();
        public bool targetingParty = false;
        
        //Input Manager stuffs
        [SerializeField] private InputActionAsset module;
        private InputActionMap inputActionMap;
        private InputAction submitAction;
        private InputAction cancelAction;
        private InputAction   moveAction;

        //Here we keep track of all the states, the order they connect and any interjects that may occur.

        //Also manage inputs here? It feels really unnecessary to move that to another class.

        public void Init(BattleSystemCore core)
        {
            inputActionMap = module.FindActionMap("Turn Battle");
            
            submitAction = inputActionMap.FindAction("Accept");
            submitAction.started += OnSubmit;
            
            cancelAction = inputActionMap.FindAction("Cancel");
            cancelAction.started += OnCancel;
            
            moveAction = inputActionMap.FindAction("Move Cursor");
            moveAction.started += OnMoveAxis;
            
            if (systemStates.Count == 0) return; //No states, no need to update.
            
            for (int i = 0; i < systemStates.Count; i++)
            {
                systemStates[i].StatePreamble(core, this); // Hand the states the core for future use.
            }
            
            systemStates[currentIndex].Init(); //Initialize the active state in the beginning?
        }
        
        public void Update()
        {
            if (systemStates.Count == 0) return; //No states, no need to update.

            if (systemStates[currentIndex].initialized)
            {
                systemStates[currentIndex].UpdateState(out bool endOfLife);
                if (endOfLife)
                {
                    int lastState = currentIndex;
                    systemStates[lastState].Disconnect(); // Are we expecting a state change here? If so, let's initialize the next state as well...
                    
                    Debug.Log($"[StateManager] : Switched states from {systemStates[lastState].stateName} to {systemStates[currentIndex].stateName}");
                    
                    // Remember that we're expecting the "currentIndex" to point to a new state at this time.
                    
                    //This is also however where we should check for interjections in the future. It'd just be to store the last id if an interject is detected and resume from it after the interject.
                    
                    systemStates[currentIndex].Init();
                }
            }
        }

        public void Disassemble()
        {
            submitAction.started -= OnSubmit;
            
            submitAction.started -= OnCancel;
            
            submitAction.started -= OnMoveAxis;
            
            if (systemStates.Count == 0) return; //No states, no need to update.
            
            for (int i = 0; i < systemStates.Count; i++)
            {
                systemStates[i].Disconnect(); //Reset all states to their beginning for use next time.
            }
        }
        
        public void SwitchActiveState(string newStateID)
        {
            for (int i = 0; i < systemStates.Count; i++)
            {
                if (systemStates[i].stateId.Equals(newStateID))
                {
                    currentIndex = i;
                    break;
                }
            }
        }

        public BattleSystemStateBase GetStateByName(string reference)
        {
            for (int i = 0; i < systemStates.Count; i++)
            {
                if (systemStates[i].stateId.Equals(reference))
                {
                    return systemStates[i];
                }
            }

            return null;
        }

        private void OnSubmit(InputAction.CallbackContext obj)
        {
            systemStates[currentIndex].OnSubmitButton(obj);
        }
        
        private void OnCancel(InputAction.CallbackContext obj)
        {
            systemStates[currentIndex].OnCancelButton(obj);
        }
        
        private void OnMoveAxis(InputAction.CallbackContext obj)
        {
            systemStates[currentIndex].OnMoveAxis(obj);
        }
    }
}
