using System.Collections.Generic;
using BattleSystem.UI;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "AI Targeting State", order = 5, menuName = "Battle States/AI Targeting State")]
    public class BattleSystemAITargetState : BattleSystemStateBase
    {
        private CameraControl cameraControl;
        private TopPanelUI topPanelUI;
        private readonly float delay = 0.9f;
        private float timer = 0;
        
        public override void Init()
        {
            cameraControl = FindObjectOfType<CameraControl>();
            topPanelUI = FindObjectOfType<TopPanelUI>();
            timer = delay;
            
            
            //The process for this state should be to first calculate the targets, then show the relevant ui
            // before passing it on to the execution state

            EntityScriptable current = battleCore.GetNextEntity();
            var personalityNode = current.GetAIComponent().GetCurrentPersonalityNode(current, battleCore.GetEntityTurnItem(current).turnsTaken);
            personalityNode.targetComp.SetOwner(current);

            if (!personalityNode.hasReadFirstLoop)
            {
                personalityNode.hasReadFirstLoop = true;
                for (int i = 0; i < personalityNode.onFirstLoopInterjects.Count; i++)
                {
                    parent.SendInterject(personalityNode.onFirstLoopInterjects[i]);
                }
            }

            AbilityScriptable selectedAbility = personalityNode.moveSelect.Evaluate(current);

            List<int> targetIndices = new List<int>();

            EntityScriptable[] allRelevantEntities;

            if (selectedAbility.targetFriendlies)
                allRelevantEntities = battleCore.enemyField;
            else
                allRelevantEntities = battleCore.partyField;
            
            if (!selectedAbility.targetAll)
                targetIndices = personalityNode.targetComp.Evaluate(allRelevantEntities);

            Debug.Log($"Entity [{current.entityName}] moveSelectComp : [{personalityNode.moveSelect.GetComponentName()}] targetingComp : [{personalityNode.targetComp.GetComponentName()}]");

            parent.targetedEntities = targetIndices;
            parent.targetingParty = !selectedAbility.targetFriendlies;
            parent.lastAbility = selectedAbility;
            
            topPanelUI.SetFoldoutState(true);

            int index = 0;
            for (int i = 0; i < battleCore.enemyField.Length; i++)
            {
                if (battleCore.enemyField[i] == null) continue;
                if (battleCore.enemyField[i].throwawayId == current.throwawayId)
                {
                    index = i;
                    break;
                }
            }

            cameraControl.CloseUpOnEnemy(index, 2f, 1.2f);

            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            timer -= Time.deltaTime;

            endOfLife = (timer <= 0);
        }

        public override void Disconnect()
        {
            battleCore.systemStateManager.SwitchActiveState("_executeState");
            
            initialized = false;
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