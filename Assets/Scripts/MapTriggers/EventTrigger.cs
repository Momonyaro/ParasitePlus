using System;
using CORE;
using UnityEngine;
using UnityEngine.Events;

namespace MapTriggers
{
    [RequireComponent(typeof(BoxCollider))]
    public class EventTrigger : MonoBehaviour
    {
        public string[] promptMessage = new string[0];
        public bool triggerActive = false;
        public bool disabled = false;
        public bool generateNewGUID = false;
        public string guid = Guid.NewGuid().ToString();
        public UnityEvent onEvent = new UnityEvent();
        
        private void OnValidate()
        {
            if (generateNewGUID)
            {
                guid = Guid.NewGuid().ToString();
                generateNewGUID = false;
            }
        }

        private void Start()
        {
            DungeonManager dm = FindObjectOfType<DungeonManager>();

            if (dm.eventTriggers.Contains(this))
                return;

            dm.eventTriggers.Add(this);
        }

        public void PlayEvent()
        {
            onEvent?.Invoke();
            InfoPrompt.Instance.CreatePrompt(promptMessage);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (disabled) return;
            if (other.CompareTag("Player"))
            {
                triggerActive = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (disabled) return;
            if (other.CompareTag("Player"))
            {
                triggerActive = false;
            }
        }
    }
}