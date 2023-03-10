using System;
using CORE;
using Scriptables;
using UnityEngine;

namespace MapTriggers
{
    [RequireComponent(typeof(BoxCollider))]
    public class EncounterTrigger : MonoBehaviour
    {
        public bool combatThroughPrompt = false;
        public bool useTrigger = true;
        public string[] promptMessage = new string[0];
        public bool triggerActive = false;
        public bool disabled = false;
        public string postBattleSceneName = "";
        public string encounterTrack = String.Empty;
        public bool generateNewGUID = false;
        public string guid = Guid.NewGuid().ToString();
        public EntityScriptable[] enemyRoster = new EntityScriptable[5];

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

            if (dm.encounterTriggers.Contains(this))
                return;

            dm.encounterTriggers.Add(this);
        }

        public void TriggerEvent()
        {
            if (combatThroughPrompt)
            {
                InfoPrompt.Instance.CreatePromptForCombat(promptMessage, this);
            }
            else
                triggerActive = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (disabled || !useTrigger) return;
            Debug.Log(other.gameObject.name);
            if (other.CompareTag("Player"))
            {
                TriggerEvent();
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