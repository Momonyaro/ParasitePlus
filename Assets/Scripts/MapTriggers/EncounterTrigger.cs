﻿using System;
using CORE;
using Scriptables;
using UnityEngine;

namespace MapTriggers
{
    [RequireComponent(typeof(BoxCollider))]
    public class EncounterTrigger : MonoBehaviour
    {
        public bool combatThroughPrompt = false;
        public string promptMessage = "";
        public bool triggerActive = false;
        public bool disabled = false;
        public string postBattleSceneName = "";
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

        private void OnTriggerEnter(Collider other)
        {
            if (disabled) return;
            Debug.Log(other.gameObject.name);
            if (other.CompareTag("Player"))
            {
                if (combatThroughPrompt)
                {
                    InfoPrompt.Instance.CreatePromptForCombat(promptMessage, this);
                }
                else
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