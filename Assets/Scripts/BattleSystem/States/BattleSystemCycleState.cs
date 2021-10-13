using BattleSystem.UI;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Clean Cycle State", order = 3, menuName = "Battle States/Clean Cycle State")]
    public class BattleSystemCycleState : BattleSystemStateBase
    {
        private BottomPanelUI bottomPanelUI;
        private TurnCounterUI turnCounterUI;
        private BattleSystemEnemyField enemyField;

        public override void Init()
        {
            //Perhaps use this to contact the appropriate UI?
            bottomPanelUI = FindObjectOfType<BottomPanelUI>();
            turnCounterUI = FindObjectOfType<TurnCounterUI>();
            enemyField = FindObjectOfType<BattleSystemEnemyField>();

            
            
            battleCore.turnOrderComponent.AddEndOfTurnPointsToQueue();
            int currentID = battleCore.turnOrderComponent.GetFirstInLine().entityId;
            battleCore.turnOrderComponent.CycleTurnQueue();

            BattleSystemSelectionState selectState = (BattleSystemSelectionState)parent.GetStateByName("_selectionState");

            // if (selectState.lastEntityPassedTurn)
            // {
            //     selectState.lastEntityPassedTurn = false;
            //     battleCore.turnOrderComponent.ModifyEntityTurnScore(currentID, 50);
            // }
            
            bottomPanelUI.PopulateOptions(new SelectableWheelOption[]
            {
                new SelectableWheelOption("", "", ""), 
            }, 0);

            turnCounterUI.PopulateTurnQueueUI(battleCore.GetTurnQueueAsEntities());
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = true;
        }

        public override void Disconnect()
        {
            initialized = false;
            
            //Check for win/loss result. If true, switch to the relevant state.
            if (CheckPlayerWinCase())
            {
                parent.SwitchActiveState("_winState");
                return;
            }
            
            //Transfer to next state
            parent.SwitchActiveState("_enemyBranch");
        }

        private bool CheckPlayerWinCase()
        {
            EntityScriptable[] enemies = battleCore.enemyField;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) continue; // If entity is null, skip them.
                
                if (!enemies[i].deadTrigger) // If entity is alive, return false win condition.
                    return false;
            }

            return true;
        }
        
        private bool CheckPlayerLossCase()
        {
            EntityScriptable[] playerParty = battleCore.partyField;

            for (int i = 0; i < playerParty.Length; i++)
            {
                if (playerParty[i] == null) continue; // If entity is null, skip them.
                
                if (!playerParty[i].deadTrigger) // If entity is alive, return false lose condition.
                    return false;
            }

            return true;
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
