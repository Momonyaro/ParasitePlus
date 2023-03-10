using System;
using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Startup State", order = 0, menuName = "Battle States/Startup State")]
    public class BattleSystemStartupState : BattleSystemStateBase
    {
        private BottomPanelUI bottomPanelUI;
        private TopPanelUI topPanelUI;
        private TurnCounterUI turnCounterUI;
        private BattleSystemEnemyField enemyField;

        [Range(0.1f, 2.0f)]
        public float startTimer = 1.2f;

        private float timer = 0;
        
        /// <summary>
        /// This state basically serves as a startup wait timer, here we simply allow all the enemies and UI to play
        /// their animations. When we're finished we should enter the next state where we branch depending on
        /// if the current entity is an enemy or a player character.
        /// </summary>

        public override void Init()
        {
            //Perhaps use this to contact the appropriate UI?
            bottomPanelUI = FindObjectOfType<BottomPanelUI>();
            topPanelUI = FindObjectOfType<TopPanelUI>();
            turnCounterUI = FindObjectOfType<TurnCounterUI>();
            enemyField = FindObjectOfType<BattleSystemEnemyField>();
            
            bottomPanelUI.PopulateOptions(new []
            {
                new SelectableWheelOption("Skills", "", "_viewSkills", "View your skills."), 
                new SelectableWheelOption("Item", "", "_viewItems", "View your items."), 
                new SelectableWheelOption("Pass", "", "_passTurn", "Pass the current turn (get placed higher in the turn queue)."), 
            }, 0);
            
            enemyField.WipeField();
            enemyField.PopulateField(battleCore.enemyField);

            topPanelUI.PopulatePartyCards(battleCore.GetPlayerParty(), battleCore.turnOrderComponent.GetFirstInLine().entityId);
            
            turnCounterUI.PopulateTurnQueueUI(battleCore.GetTurnQueueAsEntities());
            
            timer = startTimer;
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            if (Math.Abs(timer - startTimer) < float.Epsilon)
                enemyField.ShadeEntireField();
            
            timer -= Time.deltaTime;

            endOfLife = (timer <= 0);
        }

        public override void Disconnect()
        {
            timer = 0;
            
            initialized = false;
            
            //Transfer to next state
            parent.SwitchActiveState("_enemyBranch");
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            //This could perhaps skip this state, directly move to the next one? Not like we're doing anything in particular...
            timer = 0;
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            //Not needed in this state.
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            //Not needed in this state.
        }
    }
}
