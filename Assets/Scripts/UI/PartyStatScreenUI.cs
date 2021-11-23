using System;
using System.Collections.Generic;
using BattleSystem;
using CORE;
using MOVEMENT;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class PartyStatScreenUI : MonoBehaviour
    {
        public Animator menuAnimator;
        public Transform menuParent;
        public GameObject[] listButtons;
        public Sprite[] portraits;
        
        
        public Image portrait;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI xpReqText;
        public TextMeshProUGUI healthText;
        public Image healthFill;
        public TextMeshProUGUI apText;
        public Image apFill;
        public List<GameObject> abilityList = new List<GameObject>();

        private void Awake()
        {
            UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
        }


        public void ListenForMessage(string msg)
        {
            MapManager mapManager = FindObjectOfType<MapManager>();
            
            switch (msg)
            {
                case "_showPartyStatScreen":
                    SetMenuVisibility(true);
                    break;
                case "_hidePartyStatScreen":
                    SetMenuVisibility(false);
                    break;
                
                case "_viewPartyStats1":
                    UpdatePartyStatShowcase(mapManager.currentSlimData.partyField[0]);
                    break;
                case "_viewPartyStats2":
                    UpdatePartyStatShowcase(mapManager.currentSlimData.partyField[1]);
                    break;
                case "_viewPartyStats3":
                    UpdatePartyStatShowcase(mapManager.currentSlimData.partyField[2]);
                    break;
            }
        }
        
        private void SetMenuVisibility(bool visibility)
        {
            MapManager mapManager = FindObjectOfType<MapManager>();
            
            if (visibility)
            {
                for (int i = 0; i < listButtons.Length; i++)
                {
                    bool exists = (mapManager.currentSlimData.partyField[i] != null);
                    listButtons[i].SetActive(exists);
                    if (exists)
                    {
                        Transform unselected = listButtons[i].transform.GetChild(0);
                        Transform selected = listButtons[i].transform.GetChild(1);

                        unselected.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                            mapManager.currentSlimData.partyField[i].entityName.ToUpper();
                        
                        selected.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                            mapManager.currentSlimData.partyField[i].entityName.ToUpper();
                    }
                }
                
                EventSystem.current.SetSelectedGameObject(listButtons[0]);
                UpdatePartyStatShowcase(mapManager.currentSlimData.partyField[0]);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            menuAnimator.SetBool("Active", visibility);
        }

        private void UpdatePartyStatShowcase(EntityScriptable scriptable)
        {
            switch (scriptable.entityId)
            {
                case "_player":
                    portrait.sprite = portraits[0];
                    break;
                case "_gummo":
                    portrait.sprite = portraits[1];
                    break;
                case "_sandra":
                    portrait.sprite = portraits[2];
                    break;
                case "_sive":
                    portrait.sprite = portraits[3];
                    break;
            }
            
            nameText.text = scriptable.entityName;
            levelText.text = $"Lv. {scriptable.entityLevel}";
            xpReqText.text = $"XP: {scriptable.entityXpThreshold - scriptable.entityXp}";
            
            healthText.text = $"{scriptable.GetEntityHP().x}/{scriptable.GetEntityHP().y}";
            healthFill.fillAmount = scriptable.GetEntityHP().x/(float)scriptable.GetEntityHP().y;
            
            apText.text = $"{scriptable.GetEntityAP().x}/{scriptable.GetEntityAP().y}";
            apFill.fillAmount = scriptable.GetEntityAP().x/(float)scriptable.GetEntityAP().y;

            for (int i = 0; i < abilityList.Count; i++)
            {
                abilityList[i].SetActive(true);
                if (scriptable.abilityScriptables.Length <= i)
                {
                    abilityList[i].SetActive(false);
                    continue;
                }
                if (scriptable.abilityScriptables[i] == null)
                {
                    abilityList[i].SetActive(false);
                    continue;
                }
                abilityList[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = scriptable.abilityScriptables[i].abilityName;
                abilityList[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = scriptable.abilityScriptables[i].abilityDesc;
            }
        }
    }
}
