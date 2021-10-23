using Scriptables;
using UnityEngine;

namespace BattleSystem.Interjects
{
    [CreateAssetMenu(fileName = "KillAllEnemiesInterject", menuName = "Interjects/KillAllEnemiesInterject", order = 1)]
    public class KillAllEnemiesInterject : InterjectBase
    {
        private BattleSystemEnemyField enemyField;
        
        public override void Init()
        {
            enemyField = FindObjectOfType<BattleSystemEnemyField>();
            for (int i = 0; i < battleCore.enemyField.Length; i++)
            {
                if (battleCore.enemyField[i] == null) continue;
                battleCore.enemyField[i].SetEntityHP(new Vector2Int(0, battleCore.enemyField[i].GetEntityHP().y));
                enemyField.EnemyDeathFade(i);
            }

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