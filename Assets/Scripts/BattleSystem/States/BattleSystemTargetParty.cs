using System.Collections.Generic;
using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Target Party State", order = 5, menuName = "Battle States/Target Party State")]
    public class BattleSystemTargetParty : BattleSystemStateBase
    {
        private TopPanelUI topPanelUI;
        private TargetingUI targetingUI;

        public bool returnToSelection = false;
        public bool confirmTargets = false;
        public int targetIndex = 0;
        
        public override void Init()
        {
            topPanelUI = FindObjectOfType<TopPanelUI>();
            targetingUI = FindObjectOfType<TargetingUI>();
            
            topPanelUI.SetFoldoutState(true);
            topPanelUI.MoveToNextCard(-1, 1);
            parent.targetingParty = true;
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            Vector2 newPos = topPanelUI.GetCardPosition(targetIndex, out bool success);
            if (success) 
                targetingUI.SetCursorPosAndVisibility(newPos, true, false);
            
            endOfLife = false;

            if (returnToSelection || confirmTargets)
                endOfLife = true;
        }

        public override void Disconnect()
        {
            if (targetingUI != null)
                targetingUI.SetCursorPosAndVisibility(Vector2.zero, false, false);

            
            initialized = false;
            
            if (confirmTargets)
            {
                confirmTargets = false;
                
                //Check for selected ability targetAll
                parent.targetedEntities = new List<int>
                {
                    targetIndex
                };
                
                targetIndex = 0;

                parent.SwitchActiveState("_executeState");
            }
            
            targetIndex = 0;

            if (returnToSelection)
            {
                returnToSelection = false;
                topPanelUI.SetFoldoutState(false);
                parent.SwitchActiveState("_selectionState");
            }
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            confirmTargets = true;
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            returnToSelection = true;
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            Vector2 movementVec = obj.ReadValue<Vector2>();
            int roundX = Mathf.RoundToInt(movementVec.x);

            targetIndex = topPanelUI.MoveToNextCard(targetIndex, roundX);
        }
    }
}
