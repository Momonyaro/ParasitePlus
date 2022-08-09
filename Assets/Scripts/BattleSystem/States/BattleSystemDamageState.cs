using System;
using System.Collections.Generic;
using BattleSystem.UI;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Damage Report State", order = 5, menuName = "Battle States/Damage Report State")]
    public class BattleSystemDamageState : BattleSystemStateBase
    {
        private BattleSystemEnemyField enemyField;
        private DamageEffectUI damageEffectUI;
        private TopPanelUI topPanelUI;

        public float enemyShakeTimeScale = 5.0f;
        public float enemyShakeMagnitude = 0.5f;
        
        [Space()]
        
        public float delay = 0.9f;
        private float timer;
        
        public override void Init()
        {
            enemyField = FindObjectOfType<BattleSystemEnemyField>();
            damageEffectUI = FindObjectOfType<DamageEffectUI>();
            topPanelUI = FindObjectOfType<TopPanelUI>();
            timer = delay;

            List<int> targetIndices = parent.targetedEntities;

            DealDamageToTargets(targetIndices);

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
            
            parent.SwitchActiveState("_restState");
        }

        private void DealDamageToTargets(List<int> indices)
        {
            AbilityScriptable abilityInUse = parent.lastAbility;
            EntityScriptable currentEntity = battleCore.GetNextEntity();

            Vector2Int currentAP = currentEntity.GetEntityAP();
            Vector2Int currentHP = currentEntity.GetEntityHP();
            currentAP.x -= abilityInUse.abilityCosts.x;
            currentHP.x -= abilityInUse.abilityCosts.y;
            
            currentEntity.SetEntityAP(currentAP);
            currentEntity.SetEntityHP(currentHP);
            
            bool isEnemy = battleCore.turnOrderComponent.GetFirstInLine().enemyTag;
            for (int i = 0; i < indices.Count; i++)
            {
                Vector2Int damageVec = Vector2Int.zero;

                // Equation is ->  y = damage + (scaling * sqrt(level))
                float damScale = abilityInUse.levelScalingDamage * Mathf.Sqrt(currentEntity.entityLevel);

                float hpSign = Mathf.Sign(abilityInUse.abilityDamage.x);
                float apSign = Mathf.Sign(abilityInUse.abilityDamage.y);

                //This is used to make sure that attacks with np mp damage doesn't get a magic value due to levelscaling
                int hpZero = Convert.ToInt32(abilityInUse.abilityDamage.x != 0);
                int apZero = Convert.ToInt32(abilityInUse.abilityDamage.y != 0);

                int scaledHpDam = Mathf.FloorToInt((Mathf.Abs(abilityInUse.abilityDamage.x) + (damScale * hpZero)) * hpSign);
                //AP not guaranteed to be zero because we're adding the damScale
                int scaledApDam = Mathf.FloorToInt((Mathf.Abs(abilityInUse.abilityDamage.y) + (damScale * apZero)) * apSign);

                damageVec.x = scaledHpDam;
                damageVec.y = scaledApDam;

                bool crit = (Random.value < abilityInUse.abilityCritChance);
                bool miss = (Random.value < abilityInUse.abilityMissChance) && !crit;

                if (crit)
                {
                    damageVec.x = Mathf.FloorToInt(damageVec.x * 1.5f);
                    damageVec.y = Mathf.FloorToInt(damageVec.y * 1.5f);
                }

                if (miss)
                {
                    damageVec = Vector2Int.zero; // Yeah so, we like to do a little trolling.
                }
                
                //How do we refer to the actual entity data of the target?
                EntityScriptable target = null;
                bool targetEnemies = false;
                if (isEnemy)
                {
                    if (abilityInUse.targetFriendlies) // The entity is an enemy
                    {
                        //Target enemies
                        target = battleCore.enemyField[indices[i]];
                        targetEnemies = true;
                    }
                    else
                    {
                        //Target party
                        target = battleCore.GetPlayerParty()[indices[i]];
                    }
                }
                else //The entity is a party member
                {
                    if (abilityInUse.targetFriendlies)
                    {
                        //Target party
                        target = battleCore.GetPlayerParty()[indices[i]];
                    }
                    else
                    {
                        //Target enemies
                        target = battleCore.enemyField[indices[i]];
                        targetEnemies = true;
                    }
                }

                bool resist = false;
                bool weak = false;
                if (!abilityInUse.targetFriendlies)
                {
                    //Find weakness/resistances
                    int attackTypeIndex = 0;
                    for (int j = 0; j < abilityInUse.damageType.Length; j++)
                    {
                        if (abilityInUse.damageType[j] > 0.5f)
                        {
                            attackTypeIndex = j;
                        }
                    }

                    float targetResistance = target.weaknesses[attackTypeIndex];
                    resist = (targetResistance < 0.9f);
                    weak = (targetResistance > 1.1f);

                    //Only HP damage scales with resistances
                    damageVec.x = Mathf.FloorToInt(damageVec.x * targetResistance);
                }
                

                Vector2Int hp = target.GetEntityHP();
                Vector2Int ap = target.GetEntityAP();
                target.SetEntityHP(new Vector2Int(Mathf.Clamp(hp.x - damageVec.x, 0, hp.y), hp.y));
                target.SetEntityAP(new Vector2Int(Mathf.Clamp(ap.x - damageVec.y, 0, ap.y), ap.y));
                target.lastAttacker = currentEntity;

                Vector2 targetScreenPos;

                if (!targetEnemies) //Target Party
                {
                    targetScreenPos = topPanelUI.GetCardPosition(indices[i], out bool exists);
                    topPanelUI.ShakePlayerCard(indices[i]);
                }
                else
                {
                    targetScreenPos = enemyField.GetEntityPosAsScreenPos(indices[i]);
                }

                SAMSARA.Samsara.Instance.PlaySFXRandomTrack(abilityInUse.abilitySoundEffect, out bool success);


                damageEffectUI.CreateAbilityEffect(targetScreenPos, abilityInUse.abilityEffectRef);
                damageEffectUI.CreateDamageSpatter(targetScreenPos, damageVec.x, damageVec.y, crit, weak, resist, !abilityInUse.hideDamageText);
                
                //Check if the enemy is dead?
                if (!target.deadTrigger)
                    DoEnemyDamageShake(indices[i], targetEnemies);
                else
                {
                    DoEnemyDeathFade(indices[i], targetEnemies);
                    for (int j = 0; j < target.onDeathInterjects.Count; j++)
                    {
                        if (target.onDeathInterjects[j] == null) continue;
                        parent.SendInterject(target.onDeathInterjects[j]);
                    }
                }
            }
        }

        private void DoEnemyDamageShake(int index, bool targetEnemyField)
        {
            //Perhaps change so that this is done via a DealDamage method instead.
            if (targetEnemyField)
                enemyField.EnemyDamageShake(index, enemyShakeTimeScale, enemyShakeMagnitude);
        }

        private void DoEnemyDeathFade(int index, bool targetEnemyField)
        {
            if (targetEnemyField)
                enemyField.EnemyDeathFade(index);
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