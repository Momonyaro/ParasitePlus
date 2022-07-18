using System.Collections.Generic;
using System.Linq;
using BattleSystem.UI;
using Items;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Selection State", order = 2, menuName = "Battle States/Selection State")]
    public class BattleSystemSelectionState : BattleSystemStateBase
    {
        private BottomPanelUI bottomPanelUI;
        public bool passDissconnect = false;
        public bool passTargetParty = false;
        public bool passTargetEnemy = false;
        public bool lastEntityPassedTurn = false;

        public override void Init()
        {
            bottomPanelUI = FindObjectOfType<BottomPanelUI>();
            
            bottomPanelUI.SetMenuVisibility(true);

            ConstructMainMenu();
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = false;
            
            if (passDissconnect || passTargetParty || passTargetEnemy)
            {
                endOfLife = true;
            }
        }

        public override void Disconnect()
        {
            if (bottomPanelUI != null && !(passTargetParty || passTargetEnemy))
                bottomPanelUI.PopulateOptions(new []
                {
                    new SelectableWheelOption("", "", ""), 
                }, 0);

            initialized = false;
            
            if (passDissconnect)
            {
                passDissconnect = false;
                
                //Give 50 if they pass the turn since they get 100 by the end of each turn.
                lastEntityPassedTurn = true;
                parent.SwitchActiveState("_restState");
            }

            if (passTargetParty)
            {
                passTargetParty = false;
                
                parent.SwitchActiveState("_targetParty");
            }
            
            if (passTargetEnemy)
            {
                passTargetEnemy = false;
                
                parent.SwitchActiveState("_targetEnemy");
            }
            
        }


        private void CompareInputToOptions(string input)
        {
            switch (input)
            {
                case "_attack":
                    AbilityScriptable test = battleCore.GetNextEntity().defaultAttack;
                    parent.lastAbility = test;
                    Debug.Log(battleCore.GetNextEntity().entityName);
                    if (test.targetFriendlies)
                        passTargetParty = true;
                    else
                        passTargetEnemy = true;
                    return;
                case "_viewSkills":
                    ConstructSkillMenu();
                    return;
                case "_viewItems":
                    ConstructItemAidMenu();
                    return;
                case "_passTurn":
                    //Also modify turn points here?
                    passDissconnect = true;
                    return;
                case "_back":
                    ConstructMainMenu();
                    break;
            }

            if (input.Contains("_skillKey"))
            {
                string cropped = input.Replace("_skillKey", "");
                //cropped should now contain the abilityId of what we selected. Proceed to targeting mode based on the ability

                AbilityScriptable toConsider = battleCore.GetNextEntity().GetAbilityByID(cropped);
                EntityScriptable currentEntity = battleCore.GetNextEntity();

                if (currentEntity.GetEntityAP().x < toConsider.abilityCosts.x) //Too expensive, last resort measure. Show this in the ui as well.
                    return;

                parent.lastAbility = toConsider;
                
                passTargetParty = parent.lastAbility.targetFriendlies;
                passTargetEnemy = !passTargetParty;
                return;
            }
            
            if (input.Contains("_itemKey"))
            {
                string cropped = input.Replace("_itemKey", "");
                //cropped should now contain the abilityId of what we selected. Proceed to targeting mode based on the ability

                AbilityScriptable a = null;
                for (int i = 0; i < battleCore.partyInventory.Count; i++)
                {
                    if (cropped.Equals(battleCore.partyInventory[i].guid))
                    {
                        Item current = battleCore.partyInventory[i];
                        a = current.itemAbility.Copy();
                        if (current.stackable && current.StackSize.x > 1)
                        {
                            current.StackSize.x--;
                        }
                        else
                            battleCore.partyInventory.RemoveAt(i);
                        break;
                    }
                }

                parent.lastAbility = a;
                passTargetParty = parent.lastAbility.targetFriendlies;
                passTargetEnemy = !passTargetParty;
                return;
            }
        }

        private void ConstructMainMenu()
        {
            bottomPanelUI.PopulateOptions(new []
            {
                new SelectableWheelOption("Attack", "_attack", "Execute a default weaker attack."), 
                new SelectableWheelOption("Skills", "_viewSkills", "View your skills."), 
                new SelectableWheelOption("Item", "_viewItems", "View your items."), 
                new SelectableWheelOption("Pass", "_passTurn", "Pass the current turn (get placed higher in the turn queue)."), 
            }, 0);
        }

        private void ConstructSkillMenu()
        {
            EntityScriptable currentEntity = battleCore.GetNextEntity();

            var options = new List<SelectableWheelOption>()
            {
                new SelectableWheelOption("Back", "_back", "Go back to the main options.")
            };

            AbilityScriptable[] scriptables = currentEntity.GetEntityAbilities();

            for (int i = 0; i < scriptables.Length; i++)
            {
                options.Add(new SelectableWheelOption(scriptables[i].abilityName, "_skillKey" + scriptables[i].abilityId, scriptables[i].abilityDesc));
            }

            bottomPanelUI.PopulateOptions(options.ToArray(), 0);
        }

        private void ConstructItemAidMenu()
        {
            var options = new List<SelectableWheelOption>()
            {
                new SelectableWheelOption("Back", "_back", "Go back to the main options.")
            };

            List<Item> items = battleCore.partyInventory;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].type != ItemType.AID) continue;
                
                options.Add(new SelectableWheelOption($"{items[i].StackSize.x}x " + items[i].name, "_itemKey" + items[i].guid, items[i].itemAbility.abilityDesc));
            }
            
            bottomPanelUI.PopulateOptions(options.ToArray(), 0);
        }
        

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            // Used to fetch the currently selected option and parse it.
            Debug.Log($"[StateManager] : [{stateName}] Comparing reference {bottomPanelUI.GetSelectedReference()} to options;");
            CompareInputToOptions(bottomPanelUI.GetSelectedReference());
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            // This will navigate the ui
            
            Vector2 movementVec = obj.ReadValue<Vector2>();
            int roundY = -Mathf.RoundToInt(movementVec.y);
            //Debug.Log($"MvmtAxis; [{obj.ReadValue<Vector2>()}], RoundY; [{roundY}]");
            
            bottomPanelUI.NavigateOptions(roundY);
        }
    }
}
