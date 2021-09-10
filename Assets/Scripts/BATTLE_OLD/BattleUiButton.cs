using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BATTLE
{
    public class BattleUiButton : MonoBehaviour
    {
        public Button btnComponent;
        public bool unfolds;
        private bool flipTextColor;
        public string btnSelectCmd;
        public string btnPressedCmd;
        private BattleUI masterComponent;
        
        private void Awake()
        {
            btnComponent = GetComponent<Button>();
            masterComponent = FindObjectOfType<BattleUI>();
        }

        private void Start()
        {
            if (btnPressedCmd == "_exitBattle")
            {
                masterComponent.Pain(gameObject);
            }
        }

        public void TieCommandSending()
        {
            masterComponent.SendCommand(btnPressedCmd);
        }

        private void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == gameObject)
            {
                btnComponent.transform.GetChild(0).GetComponent<Text>().color = Color.black;
                
                if (!flipTextColor)
                {
                    // We need to decide how we will populate the menus, whether we use enums or some lists.
                    masterComponent.SendCommand(btnSelectCmd);
                }
                
                flipTextColor = true;
            }
            else if (flipTextColor)
            {
                btnComponent.transform.GetChild(0).GetComponent<Text>().color = Color.white;
                flipTextColor = false;
            }
        }

        public void SetText(string text)
        {
            btnComponent.transform.GetChild(0).GetComponent<Text>().text = text;
        }

        public void SetOnPressCmd(string cmdString)
        {
            btnPressedCmd = cmdString;
        }
        
        public void SetOnSelectCmd(string cmdString)
        {
            btnSelectCmd = cmdString;
        }
    }
}