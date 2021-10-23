using System.Collections;
using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Rest State", order = 3, menuName = "Battle States/Rest State")]
    public class BattleSystemRestState : BattleSystemStateBase
    {
        private TopPanelUI topPanelUI;
        [Range(0.1f, 2.0f)]
        public float startTimer = 1.2f;

        public bool folded = false;

        private float timer = 0;
        
        public override void Init()
        {
            topPanelUI = FindObjectOfType<TopPanelUI>();
            timer = startTimer;

            folded = false;
            
            topPanelUI.PopulatePartyCards(battleCore.partyField, battleCore.turnOrderComponent.GetFirstInLine().entityId);
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            timer -= Time.deltaTime;

            if (timer < startTimer * 0.5f && !folded)
            {
                topPanelUI.SetFoldoutState(false);
                folded = true;
            }

            endOfLife = (timer <= 0);
        }

        public override void Disconnect()
        {
            timer = 0;

            folded = false;
            
            initialized = false;
            
            //Transfer to next state
            parent.SwitchActiveState("_cycleState");
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            //This could perhaps skip this state, directly move to the next one? Not like we're doing anything in particular...
            timer = 0;
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            
        }
    }
}
