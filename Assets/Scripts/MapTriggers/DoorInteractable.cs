using System;
using System.Collections.Generic;
using CORE;
using MOVEMENT;
using UnityEditor;
using UnityEngine;

namespace MapTriggers
{
    public class DoorInteractable : MonoBehaviour
    {
        public Vector3 dir = new Vector3();
        public bool calcDefaultSensorPos = false;
        public Vector3 sensor = new Vector3();
        public bool calcDefaultWarpDest = false;
        public Vector3 warpDest = new Vector3();
        public float interactAngleOffset = 0.1f;
        public float interactRange = .5f;

        private Transform playerTransform;
        
        public bool triggerActive = false;
        [SerializeField] private float dot;
        public bool locked = false;
        public bool singleUseKey = false;
        public string keyItemID = "";
        public bool goToScene = false;
        public string sceneName = "";
        public string loadSceneVariable = "";
        public string isLockedMessage = "The door is locked.";
        public string onUnlockMessage = "Unlocked the door using $ITEM.";
        public string interactPromptMsg = "Open Door";

        private void OnValidate()
        {
            if (sensor == Vector3.zero)
                calcDefaultSensorPos = true;
            if (calcDefaultWarpDest)
            {
                calcDefaultWarpDest = false;
                warpDest = transform.position - dir;
            }

            if (calcDefaultSensorPos)
            {
                calcDefaultSensorPos = false;
                sensor = transform.position + dir;
            }
        }

        private void Awake()
        {
            playerTransform = FindObjectOfType<FPSGridPlayer>().transform;
        }

        private void Start()
        {
            DungeonManager dm = FindObjectOfType<DungeonManager>();
            List<DoorInteractable> list = dm.doorInteractables;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == this)
                    return;
            }
            
            dm.doorInteractables.Add(this);
        }

        private void Update()
        {
            triggerActive = CheckIfInteractable();
        }

        bool CheckIfInteractable()
        {
            dot = Vector3.Dot(dir, playerTransform.forward);
            bool lookingAtTarget = false;
            bool inRange = false;

            dot += 1; // Should give us 0 since if we're looking at the door the dot product should be -1

            
            if (dot <= interactAngleOffset && dot >= -interactAngleOffset) 
                lookingAtTarget = true; 

            if (Vector3.Distance(sensor, playerTransform.position) < interactRange)
                inRange = true;
            
            return (inRange && lookingAtTarget);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = triggerActive ? Color.green : Color.blue;
            if (locked)
                Gizmos.color = Color.red;
            Vector3 pos = sensor;
            Gizmos.DrawLine(pos, pos + dir * 0.3f);
            Gizmos.DrawIcon(warpDest, "d_Import@2x");
        }
    }
}