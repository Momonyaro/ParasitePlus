using System;
using System.Collections;
using System.Security.Cryptography;
using Scriptables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BATTLE
{
    public class BattleUI : MonoBehaviour
    {
        /* Here we need a couple of main things. We need to create the main action menu and set the top option as the active one.
         * We secondly need to populate the "folders" of menu options e.g. "Abilities" or "Items"
         * We lastly need the selected option to feed into the combat system and actually execute the desired action.
         */

        public BattleManager battleManager;
        [Space] public BattleUiButton[] mainButtons;
        public GameObject skillBtnPrefab;
        public Transform skillListParent;
        public BattleUiButton[] skillButtons;
        private bool lastDeadAirState = false;

        public void OnUiEnable()
        {
            StartCoroutine(SelectButtonFix(mainButtons[0].gameObject));
        }

        private void Update()
        {
            if (lastDeadAirState == battleManager.flags[4]) return;
            lastDeadAirState = battleManager.flags[4];
            for (int i = 0; i < mainButtons.Length; i++)
            {
                mainButtons[i].btnComponent.interactable = !lastDeadAirState;
            }

            for (int i = 0; i < skillButtons.Length; i++)
            {
                skillButtons[i].btnComponent.interactable = !lastDeadAirState;
            }
        }

        public IEnumerator SelectButtonFix(GameObject buttonObject)
        {
            yield return null;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(buttonObject);
        }

        public void Pain(GameObject painObject)
        {
            StartCoroutine(SelectButtonFix(painObject));
        }
        
        //We need to fetch the current active entity's abilities now.
        private void PopulateSkillMenu()
        {
            for (int i = 0; i < skillListParent.childCount; i++)
            {
                Destroy(skillListParent.GetChild(i).gameObject);
            }
            skillButtons = new BattleUiButton[0];
            
            AbilityScriptable[] abilities = battleManager.GetActiveEntityAbilities();
            skillButtons = new BattleUiButton[abilities.Length];
            for (int i = 0; i < abilities.Length; i++)
            {
                GameObject btn = Instantiate(skillBtnPrefab, skillListParent);
                BattleUiButton btnBtnComponent = btn.GetComponent<BattleUiButton>();
                
                btnBtnComponent.SetText(abilities[i].abilityName);
                btnBtnComponent.SetOnPressCmd("_sendSkill" + abilities[i].abilityId);
                
                skillButtons[i] = btnBtnComponent;
                Navigation nav = btnBtnComponent.btnComponent.navigation;
                nav.selectOnLeft = mainButtons[1].btnComponent;
                if (i > 0)
                {
                    Navigation prevNav = skillButtons[i - 1].btnComponent.navigation;
                    prevNav.selectOnDown = skillButtons[i].btnComponent;
                    skillButtons[i - 1].btnComponent.navigation = prevNav;
                    nav.selectOnUp = skillButtons[i - 1].btnComponent;
                }

                btnBtnComponent.btnComponent.navigation = nav;
            }

            if (abilities.Length > 0)
            {
                Navigation skillNav = mainButtons[1].btnComponent.navigation;
                skillNav.selectOnRight = skillButtons[0].btnComponent;
                mainButtons[1].btnComponent.navigation = skillNav;
            }
        }

        public void SendCommand(string cmdString)
        {
            if (cmdString.Contains("_sendSkill"))
            {
                //This command should be followed by an abilityID and should be parsed as such.
                cmdString = cmdString.Remove(0, "_sendSkill".Length);
                battleManager.ParseSkillAttackCommand(cmdString);
            }
            
            switch (cmdString)
            {
                case "_defaultAtk":
                    //This would make the battle system process the active entities default attack.
                    battleManager.DefaultAttackCommand();
                    break;
                case "_skillFoldoutHover":
                    //This makes the battle UI populate the skill submenu.
                    PopulateSkillMenu();
                    break;
                case "_skillFoldoutPressed":
                    //This de-activates the main buttons and instead activates the buttons in the skill submenu.
                    if (skillButtons.Length > 0)
                        StartCoroutine(SelectButtonFix(skillButtons[0].gameObject));
                    break;
                case "_itemFoldoutHover":
                    //This makes the battle UI populate the item submenu
                    break;
                case "_itemFoldoutPressed":
                    //This de-activates the main buttons and instead activates the buttons in the skill submenu.
                    break;
                case "_wipeSubList":
                    for (int i = 0; i < skillListParent.childCount; i++)
                    {
                        Destroy(skillListParent.GetChild(i).gameObject);
                    }
                    skillButtons = new BattleUiButton[0];
                    break;
                case "_exitBattle":
                    battleManager.ReturnToPreviousScene();
                    break;
            }
        }
    }
}
