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
        public string animParamName = "";
        public Animator objAnimator;
        public bool regenGuid = false;
        public string guid = Guid.NewGuid().ToString();
        public bool triggerActive = false;
        public bool lockTrigger = false;
        public string itemID = "";
        public string interactPromptMsg = "Pick Up";

        public bool hasAnimator => objAnimator != null;

        private void OnValidate()
        {
            if (regenGuid)
            {
                guid = Guid.NewGuid().ToString();
                regenGuid = false;
            }
        }

        private void Awake()
        {
            CORE.DungeonManager dm = FindObjectOfType<CORE.DungeonManager>();

            if (dm.groundItems.Contains(this))
                return;

            dm.groundItems.Add(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (lockTrigger) return;
            if (other.GetComponent<FPSGridPlayer>())
            {
                triggerActive = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (lockTrigger) return;
            if (other.GetComponent<FPSGridPlayer>())
            {
                triggerActive = false;
            }
        }
    }
}
