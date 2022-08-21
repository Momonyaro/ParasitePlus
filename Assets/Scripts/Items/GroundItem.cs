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
        public float interactAngleOffset = 0.1f;
        public float interactRange = .5f;
        public Transform sensor;
        public string guid = "";
        public bool triggerActive = false;
        public bool lockTrigger = false;
        public string itemID = "";
        public bool playSfx = false;
        public string itemSfx = "";
        public string interactPromptMsg = "Pick Up";
        public Vector3 dir = new Vector3();

        [SerializeField] private float dot;
        [SerializeField] private float reverseDot;
        private Transform playerTransform;

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
            playerTransform = FindObjectOfType<FPSGridPlayer>().transform;

            if (dm.groundItems.Contains(this))
                return;

            dm.groundItems.Add(this);
        }

        private void Update()
        {
            reverseDot = Vector3.Dot(dir, playerTransform.forward);
            bool lookingAwayFromTarget = false;
            reverseDot = 1 - reverseDot;

            if (reverseDot > 0.8f)
                lookingAwayFromTarget = true;

            int hiddenLayer = LayerMask.NameToLayer("Hidden");
            int defaultLayer = LayerMask.NameToLayer("Default");
            SetLayerRecursively(gameObject, (lookingAwayFromTarget) ? defaultLayer : hiddenLayer);
        }

        bool CheckIfInteractable()
        {
            dot = Vector3.Dot(dir, playerTransform.forward);
            bool lookingAtTarget = false;

            bool inRange = false;

            dot += 1; // 0 when looking at object, 2 when looking away

            if (dot <= interactAngleOffset && dot >= -interactAngleOffset)
                lookingAtTarget = true;

            if (Vector3.Distance(sensor.position, playerTransform.position) < interactRange)
                inRange = true;

            return (inRange && lookingAtTarget);
        }

        private void OnTriggerStay(Collider other)
        {
            if (lockTrigger) return;
            if (other.GetComponent<FPSGridPlayer>())
            {
                triggerActive = (CheckIfInteractable());
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

        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;

            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, newLayer);
            }
        }
    }
}
