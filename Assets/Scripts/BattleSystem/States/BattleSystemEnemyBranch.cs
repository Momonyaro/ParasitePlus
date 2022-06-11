using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Enemy Branch State", order = 1, menuName = "Battle States/Enemy Branch State")]
    public class BattleSystemEnemyBranch : BattleSystemStateBase
    {
        private TopPanelUI topPanelUI;
        private bool nextIsEnemy = false;
        private bool entityDead = false;
        
        public override void Init()
        {
            topPanelUI = FindObjectOfType<TopPanelUI>();
            //Basically check if the next entity in line is tagged as an enemy or not, if it is we go to the enemy AI states.
            // else we go to the player's UI based states.

            nextIsEnemy = battleCore.turnOrderComponent.GetFirstInLine().enemyTag; // Currently not null-checking...
            
            //Check if the entity is dead and if so, we just skip straight to the next entity.
            entityDead = battleCore.GetNextEntity().deadTrigger;
            
            topPanelUI.PopulatePartyCards(battleCore.GetPlayerParty(), battleCore.turnOrderComponent.GetFirstInLine().entityId);
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = true;
        }

        public override void Disconnect()
        {

            //Switch state based on "nextIsEnemy"
            

            initialized = false;

            if (entityDead)
            {
                parent.SwitchActiveState("_cycleState");
                return;
            }
            
            if (nextIsEnemy) //Replace later with enemy target state!
                parent.SwitchActiveState("_aiTargetState");
            else
                parent.SwitchActiveState("_selectionState");
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            
        }
    }
}
