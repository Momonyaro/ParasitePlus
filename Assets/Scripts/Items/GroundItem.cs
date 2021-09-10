using System;
using MOVEMENT;
using UnityEngine;

namespace Items
{
    public class GroundItem : MonoBehaviour
    {
        //This component will hold a reference to a item and when the player interacts with this item
        // we get the item from the database and place it in the player's inventory.
        
        //Later on, if the player's inventory is full a prompt will appear telling the player that
        // adding the item failed.
        
        // Start is called before the first frame update
        public bool regenGuid = false;
        public string guid = Guid.NewGuid().ToString();
        public Animator beamAnimator;
        public bool triggerActive = false;
        public string itemID = "";

        private static string _animPromptVariable = "ShowPrompt";

        private void OnValidate()
        {
            if (regenGuid)
            {
                guid = Guid.NewGuid().ToString();
                regenGuid = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<FPSGridPlayer>())
            {
                beamAnimator.SetBool(_animPromptVariable, true);
                triggerActive = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<FPSGridPlayer>())
            {
                beamAnimator.SetBool(_animPromptVariable, false);
                triggerActive = false;
            }
        }
    }
}
