using System;
using CORE;
using UnityEngine;

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
        
        private void OnValidate()
        {
            if (generateNewGUID)
            {
                guid = Guid.NewGuid().ToString();
                generateNewGUID = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (disabled || !triggerActive) return;
            if (other.CompareTag("Player"))
            {
                InfoPrompt.Instance.CreatePrompt(promptMessage);
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