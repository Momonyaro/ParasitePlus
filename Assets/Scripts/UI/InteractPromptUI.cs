using System;
using CORE;
using Items;
using MapTriggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InteractPromptUI : MonoBehaviour
    {
        //So basically scan the DungeonManager for it's interactables, if one if active then activate the hud and display the item's message.
        public DungeonManager dm;
        public TextMeshProUGUI promptText;
        public Animator promptAnimator;

        private void Awake()
        {
            dm = FindObjectOfType<DungeonManager>();
        }

        private void Update()
        {
            bool promptActive = false;
            DoorInteractable doorFound = dm.CheckForActiveDoor();

            if (doorFound != null)
            {
                promptText.text = doorFound.interactPromptMsg;
                promptActive = true;
            }
            MapInteractable interactableFound = dm.CheckForInteractable();

            if (interactableFound != null)
            {
                promptText.text = interactableFound.interactPromptMsg;
                promptActive = true;
            }


            GroundItem itemFound = dm.CheckForActiveGroundItem();
            if (itemFound != null)
            {
                promptText.text = itemFound.interactPromptMsg;
                promptActive = true;
            }

            if (dm.GetPlayer().lockPlayer)
                promptActive = false;
            
            promptAnimator.SetBool("Active", promptActive);
        }
    }
}
