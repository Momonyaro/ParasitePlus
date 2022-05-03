using System;
using CORE;
using MOVEMENT;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class StatusMenuUI : MonoBehaviour
    {
        public Animator menuAnimator;
        public GameObject resumeBtn;
        public GameObject miniMap;

        public Transform cardHolder;
        public GameObject cardPrefab;
        public TextMeshProUGUI walletText;
        public Sprite playerPortrait;
        public Sprite gummoPortrait;
        public bool lockMenu = false;

        //We need a method to show the menu, to hide the menu and a lock so that if we have sub-menus we don't hide the main status menu.

        private void Start()
        {
        }

        private void OnEnable()
        {
            UIManager.Instance.onUIMessage.AddListener(ListenForMessages);
        }

        private void OnDisable()
        {
            UIManager.Instance.onUIMessage.RemoveListener(ListenForMessages);
        }
        
        private void OnDestroy()
        {
            UIManager.Instance.onUIMessage.RemoveListener(ListenForMessages);
        }

        private void SetMenuVisibility(bool visibility)
        {
            if (lockMenu)
                return;
            
            if (visibility)
            {
                //Populate menu with data from slim
                PopulateMenu();
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(null);
                FindObjectOfType<FPSGridPlayer>().lockPlayer = false;
            }
            
            miniMap.SetActive(!visibility);
            
            menuAnimator.SetBool("Active", visibility);
        }

        public void ListenForMessages(string message)
        {
            Debug.Log("message recived:" + message);
            switch (message)
            {
                case "_toggleStatusMenu":
                    SetMenuVisibility(!menuAnimator.GetBool("Active"));
                    break;
                
                case "_lockStatusMenu":
                    lockMenu = true;
                    break;
                
                case "_unlockStatusMenu":
                    lockMenu = false;
                    break;
                
                case "_showPartyStatScreen":
                    lockMenu = true;
                    break;
                
                case "_hidePartyStatScreen":
                    PopulateMenu();
                    lockMenu = false;
                    break;
                case "_quitToMainMenu":
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MAIN_MENU");
                    break;
            }
        }

        public void PopulateMenu()
        {
            EventSystem.current.SetSelectedGameObject(resumeBtn);

            MapManager mapManager = FindObjectOfType<MapManager>();

            walletText.text = $"Money: {mapManager.currentSlimData.wallet} SEK";

            for (int i = 0; i < cardHolder.childCount; i++)
            {
                Destroy(cardHolder.GetChild(i).gameObject);
            }

            EntityScriptable[] party = mapManager.currentSlimData.partyField;

            for (int i = 0; i < party.Length; i++)
            {
                if (party[i] == null) continue;
                
                AddPartyCard(party[i]);
            }

        }

        public void AddPartyCard(EntityScriptable entityScriptable)
        {
            GameObject card = Instantiate(cardPrefab, cardHolder);

            PartyCardHolderUI ui = card.GetComponent<PartyCardHolderUI>();

            ui.nameText.text = entityScriptable.entityName;
            ui.levelText.text = $"Lv. {entityScriptable.entityLevel}";
            ui.hpNumText.text = $"{entityScriptable.GetEntityHP().x} / {entityScriptable.GetEntityHP().y}";
            ui.hpBar.fillAmount = entityScriptable.GetEntityHP().x/(float)entityScriptable.GetEntityHP().y;
            ui.apNumText.text = $"{entityScriptable.GetEntityAP().x} / {entityScriptable.GetEntityAP().y}";
            ui.apBar.fillAmount = entityScriptable.GetEntityAP().x/(float)entityScriptable.GetEntityAP().y;

            Sprite portrait = null;
            switch (entityScriptable.entityId)
            {
                case "_player":
                    portrait = playerPortrait;
                    break;
                case "_gummo":
                    portrait = gummoPortrait;
                    break;
            }

            ui.portrait.sprite = portrait;
        }
    }
}
