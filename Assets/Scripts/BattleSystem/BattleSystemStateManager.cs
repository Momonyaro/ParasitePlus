using System.Collections.Generic;
using Items;
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
        [SerializeReference] public List<InterjectBase> systemInterjects = new List<InterjectBase>(); 
        [Space()]
        public int currentIndex = 0;

        public AbilityScriptable lastAbility;
        [HideInInspector] public Item lastItem = null;
        public bool hasItem = false;
        public List<int> targetedEntities = new List<int>();
        public bool targetingParty = false;
        private InterjectBase currentInterject;

        private BattleSystemCore battleCore;
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
            lastItem = null;
            hasItem = false;
            battleCore = core;
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

            if (currentInterject != null)
            {
                currentInterject.UpdateState(out bool endInterject);
                if (endInterject)
                {
                    currentInterject.Disconnect();
                    
                    currentInterject = null;
                    systemStates[currentIndex].Init();
                }
                else
                {
                    return;
                }
            }

            if (systemStates[currentIndex].initialized)
            {
                systemStates[currentIndex].UpdateState(out bool endOfLife);
                if (endOfLife)
                {
                    int lastState = currentIndex;
                    systemStates[lastState].Disconnect(); // Are we expecting a state change here? If so, let's initialize the next state as well...
                    
                    Debug.Log($"[StateManager] : Switched states from {systemStates[lastState].stateName} to {systemStates[currentIndex].stateName}");

                    // Remember that we're expecting the "currentIndex" to point to a new state at this time.

                    InterjectBase fetchedInterject = CheckForInterject(systemStates[lastState].stateId);
                    if (fetchedInterject != null)
                    {
                        currentInterject = fetchedInterject;
                        currentInterject.Init();
                        
                        return;
                    }

                    //This is also however where we should check for interjections in the future. It'd just be to store the last id if an interject is detected and resume from it after the interject.
                    
                    systemStates[currentIndex].Init();
                }
            }
        }

        public void Disassemble()
        {
            submitAction.started -= OnSubmit;

            cancelAction.started -= OnCancel;

            moveAction.started -= OnMoveAxis;
            
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

        public void SendInterject(InterjectBase interject)
        {
            interject.StatePreamble(battleCore, this);
            systemInterjects.Add(interject);
        }

        private InterjectBase CheckForInterject(string lastStateRef)
        {
            for (int i = 0; i < systemInterjects.Count; i++)
            {
                if (systemInterjects[i].prevStateId.Equals(lastStateRef))
                {
                    //It's go time!
                    InterjectBase selected = systemInterjects[i];
                    systemInterjects.RemoveAt(i);
                    return selected;
                }
            }

            return null;
        }

        private void OnSubmit(InputAction.CallbackContext obj)
        {
            if (currentInterject != null)
            {
                currentInterject.OnSubmitButton(obj);
                return;
            }
            
            systemStates[currentIndex].OnSubmitButton(obj);
        }
        
        private void OnCancel(InputAction.CallbackContext obj)
        {
            if (currentInterject != null)
            {
                currentInterject.OnCancelButton(obj);
                return;
            }
            
            systemStates[currentIndex].OnCancelButton(obj);
        }
        
        private void OnMoveAxis(InputAction.CallbackContext obj)
        {
            if (currentInterject != null)
            {
                currentInterject.OnMoveAxis(obj);
                return;
            }
            
            systemStates[currentIndex].OnMoveAxis(obj);
        }
    }
}
