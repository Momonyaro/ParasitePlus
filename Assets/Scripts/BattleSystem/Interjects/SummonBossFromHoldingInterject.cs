using Scriptables;
using UnityEngine;

namespace BattleSystem.Interjects
{
    [CreateAssetMenu(fileName = "SummonBossFromHoldingInterject", menuName = "Interjects/SummonBossFromHoldingInterject", order = 1)]
    public class SummonBossFromHoldingInterject : InterjectBase
    {
        public string bossId;
        public int spawnAtIndex = 2;
        
        private BattleSystemEnemyField enemyField;
        
        public override void Init()
        {
            enemyField = FindObjectOfType<BattleSystemEnemyField>();
            for (int i = 0; i < battleCore.enemyField.Length; i++)
            {
                if (battleCore.enemyField[i] == null) continue;
                battleCore.RemoveEntityFromBattle(battleCore.enemyField[i].throwawayId);
            }

            EntityScriptable boss = battleCore.RetrieveEntityFromHoldingCell(bossId);
            battleCore.AddEntityToBattle(spawnAtIndex, boss);
            
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
    }
}