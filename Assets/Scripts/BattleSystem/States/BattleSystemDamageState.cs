using System.Collections.Generic;
using BattleSystem.UI;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;

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
                int damage = 0;

                float damScale = abilityInUse.levelScalingDamage * currentEntity.entityLevel;
                int scaledDam = Mathf.FloorToInt(abilityInUse.abilityDamage.x + damScale);
                
                damage = scaledDam;
                bool crit = (Random.value < abilityInUse.abilityCritChance);
                bool miss = (Random.value < abilityInUse.abilityMissChance) && !crit;

                if (crit)
                {
                    damage = Mathf.RoundToInt(damage * 1.5f); // Increase damage by 1.5 if critting.
                }

                if (miss)
                {
                    damage = 0; // Yeah so, we like to do a little trolling.
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
                        target = battleCore.partyField[indices[i]];
                    }
                }
                else //The entity is a party member
                {
                    if (abilityInUse.targetFriendlies)
                    {
                        //Target party
                        target = battleCore.partyField[indices[i]];
                    }
                    else
                    {
                        //Target enemies
                        target = battleCore.enemyField[indices[i]];
                        targetEnemies = true;
                    }
                }
                
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
                bool resist = (targetResistance < 0.9f);
                bool weak = (targetResistance > 1.1f);

                damage = Mathf.FloorToInt(damage * targetResistance);

                Vector2Int hp = target.GetEntityHP();
                target.SetEntityHP(new Vector2Int(Mathf.Clamp(hp.x - damage, 0, hp.y), hp.y));
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
                
                damageEffectUI.CreateAbilityEffect(targetScreenPos, abilityInUse.abilityEffectRef);
                damageEffectUI.CreateDamageSpatter(targetScreenPos, damage, crit, weak, resist);
                
                //Check if the enemy is dead?
                if (!target.deadTrigger)
                    DoEnemyDamageShake(indices[i], targetEnemies);
                else
                    DoEnemyDeathFade(indices[i], targetEnemies);
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