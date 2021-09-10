using System.Collections.Generic;
using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Target Enemy State", order = 6, menuName = "Battle States/Target Enemy State")]
    public class BattleSystemTargetEnemy : BattleSystemStateBase
    {
        private BattleSystemEnemyField enemyField;
        private TargetingUI targetingUI;

        public bool returnToSelection = false;
        public bool confirmTargets = false;
        public int targetIndex = 0;
        
        public override void Init()
        {
            enemyField = FindObjectOfType<BattleSystemEnemyField>();
            targetingUI = FindObjectOfType<TargetingUI>();

            targetIndex = enemyField.MoveToNextEnemy(1, 1);
            parent.targetingParty = false;
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {

            Vector2 newPos = enemyField.GetEntityPosAsScreenPos(targetIndex, parent.lastAbility.targetAll);
            targetingUI.SetCursorPosAndVisibility(newPos, true, parent.lastAbility.targetAll);
            
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
                if (parent.lastAbility.targetAll)
                {
                    parent.targetedEntities = battleCore.GetIndicesInUseByEnemies();
                }
                else
                {
                    parent.targetedEntities = new List<int> { targetIndex };
                }
                
                
                targetIndex = 0;

                parent.SwitchActiveState("_executeState");
            }

            targetIndex = 0;
            
            if (returnToSelection)
            {
                returnToSelection = false;
                
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
            int roundX = Mathf.RoundToInt(Mathf.Clamp(movementVec.x, -1, 1));

            targetIndex = enemyField.MoveToNextEnemy(targetIndex, roundX);
        }
    }
}
