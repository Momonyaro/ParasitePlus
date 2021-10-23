using System;
using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace BattleSystem.Interjects
{
    [CreateAssetMenu(fileName = "ReinforcementInterject", menuName = "Interjects/ReinforcementInterject", order = 1)]
    public class ReinforcementInterject : InterjectBase
    {
        public List<EntityScriptable> reinforcements = new List<EntityScriptable>();
        public int[] toPlaceInHolding = new int[0];
        public bool scrambleEntityOrder = false;

        private BattleSystemEnemyField enemyField;
        
        public override void Init()
        {
            enemyField = FindObjectOfType<BattleSystemEnemyField>();
            List<EntityScriptable> entities = new List<EntityScriptable>(reinforcements);
            
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] == null) continue;
                entities[i] = entities[i].Copy();
            }

            for (int i = 0; i < toPlaceInHolding.Length; i++)
            {
                RemoveFromFieldAndIntoHolding(toPlaceInHolding[i]);
            }

            if (scrambleEntityOrder)
            {
                entities = Shuffle(entities, new System.Random(Mathf.FloorToInt(Time.time)));
            }

            for (int i = 0; i < entities.Count; i++)
            {
                battleCore.AddEntityToBattle(i, entities[i]);
            }
            
            enemyField.PopulateField(battleCore.enemyField);
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = true;
        }

        public override void Disconnect()
        {
            initialized = false;
        }

        private void RemoveFromFieldAndIntoHolding(int index)
        {
            battleCore.PlaceEntityInHoldingCell(battleCore.enemyField[index].Copy());
            battleCore.RemoveEntityFromBattle(battleCore.enemyField[index].throwawayId);
        }

        private static List<T> Shuffle<T>(List<T> list, System.Random rnd)
        {
            for (var i = list.Count-1; i > 0; i--)
            {
                var randomIndex = rnd.Next(i + 1); //maxValue (i + 1) is EXCLUSIVE
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }

            return list;
        }
    }
}