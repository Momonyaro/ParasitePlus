﻿using System.Collections.Generic;
using BattleSystem.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Execution State", order = 5, menuName = "Battle States/Execution State")]
    public class BattleSystemExecuteState : BattleSystemStateBase
    {

        private CameraControl cameraControl;
        private TargetingUI targetingUI;
        
        public float camTimeScale = 2.0f;
        public float camHoldTime = 1.2f;
        public float camShakeTimeScale = 5.0f;
        public float camShakeMagnitude = 0.5f;
        
        [Space()]
        
        public float delay = 0.5f;
        private float timer;
        
        public override void Init()
        {
            timer = delay;
            
            cameraControl = FindObjectOfType<CameraControl>();
            targetingUI = FindObjectOfType<TargetingUI>();

            List<int> targetIndices = parent.targetedEntities;
            
            Debug.Log($"[StateManager] : [{stateName}] found {targetIndices.Count} targets.");

            if (!parent.targetingParty) // Targeting Enemy field
            {
                EnemyExecution(targetIndices);
            }
            else                        // Targeting the Party
            {
                PartyExecution(targetIndices);
            }
            
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            timer -= Time.deltaTime;

            endOfLife = (timer <= 0);
        }

        public override void Disconnect()
        {
            timer = 0;
            
            initialized = false;
            
            parent.SwitchActiveState("_reportState");
        }

        private void PartyExecution(List<int> targetIndices)
        {
            cameraControl.CameraShake(camShakeTimeScale, camShakeMagnitude);
        }

        private void EnemyExecution(List<int> targetIndices)
        {
            if (targetIndices.Count == 1) //Single Target case
            {
                SingleTarget(targetIndices[0]);
            }
            else if (targetIndices.Count > 1) //Multi Target case
            {
                MultiTarget(targetIndices);
            }
        }

        private void SingleTarget(int index)
        {
            cameraControl.CloseUpOnEnemy(index, camTimeScale, camHoldTime);
        }

        private void MultiTarget(List<int> indices)
        {
            int median = 0;
            for (int i = 0; i < indices.Count; i++)
            {
                median += indices[i];
            }
            median /= indices.Count;
            
            cameraControl.CloseUpOnEnemy(median, camTimeScale, camHoldTime);
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            
        }
    }
}
