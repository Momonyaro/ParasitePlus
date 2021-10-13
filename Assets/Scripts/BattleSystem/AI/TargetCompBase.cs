using Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.AI
{
    public abstract class TargetCompBase
    {
        public EntityScriptable owner;
        public string componentName;

        public abstract List<int> Evaluate(EntityScriptable[] targets);

        public virtual string GetComponentName()
        {
            return "NOT IMPLEMENTED [ERR]";
        }
        public virtual string GetComponentTooltip()
        {
            return "NOT IMPLEMENTED [ERR]";
        }

        public void SetOwner(EntityScriptable entity)
        {
            owner = entity;
        }
    }

    public class TargetCompRandom : TargetCompBase
    {
        public override List<int> Evaluate(EntityScriptable[] targets)
        {
            //Look through possible enemy targets and just pick one at random.
            List<int> possibleIndices = new List<int>();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    continue;
                
                if (!targets[i].deadTrigger)
                    possibleIndices.Add(i);
            }

            return new List<int>() {possibleIndices[Random.Range(0, possibleIndices.Count)]};
        }

        public override string GetComponentName()
        {
            return "Random";
        }

        public override string GetComponentTooltip()
        {
            return "Selects an entity at random that isn't dead.";
        }
    }

    public class TargetCompBully : TargetCompBase
    {
        public override List<int> Evaluate(EntityScriptable[] targets)
        {
            List<int> possibleIndices = new List<int>();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    continue;
                
                if (!targets[i].deadTrigger)
                    possibleIndices.Add(i);
            }

            int weakestEnemyIndex = possibleIndices[Random.Range(0, possibleIndices.Count)];
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                if (targets[i].deadTrigger) continue;

                if (targets[i].GetEntityHP().x < targets[weakestEnemyIndex].GetEntityHP().x)
                    weakestEnemyIndex = i;
            }

            return new List<int> { weakestEnemyIndex };
        }

        public override string GetComponentName()
        {
            return "Bully";
        }
        public override string GetComponentTooltip()
        {
            return "Selects the entity with the lowest current HP.";
        }
    }

    public class TargetCompVengeful : TargetCompBase
    {
        public override List<int> Evaluate(EntityScriptable[] targets)
        {
            if (owner == null) { Debug.LogError("No Owner set?"); return new List<int>(); }
            
            if (owner.lastAttacker != null && !owner.lastAttacker.deadTrigger)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i].throwawayId == owner.lastAttacker.throwawayId)
                        return new List<int> { i };
                }
            }

            List<int> possibleIndices = new List<int>();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    continue;

                if (!targets[i].deadTrigger)
                    possibleIndices.Add(i);
            }

            return new List<int>() { possibleIndices[Random.Range(0, possibleIndices.Count)] };
        }

        public override string GetComponentName()
        {
            return "Vengeful";
        }

        public override string GetComponentTooltip()
        {
            return "The entity will try to attack whoever attacked it last. If no one has attacked it yet or if that attacker is dead, it will instead choose a random target.";
        }
    }

    public class TargetCompKingslayer : TargetCompBase
    {
        public override List<int> Evaluate(EntityScriptable[] targets)
        {
            List<int> possibleIndices = new List<int>();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    continue;

                if (!targets[i].deadTrigger)
                    possibleIndices.Add(i);
            }

            int strongestEnemyIndex = possibleIndices[Random.Range(0, possibleIndices.Count)];
            int highScore = 0;
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] == null) continue;
                if (targets[i].deadTrigger) continue;

                int newScore = targets[i].entityLevel * targets[i].GetEntityHP().x;

                if (highScore < newScore)
                {
                    highScore = newScore;
                    strongestEnemyIndex = i;
                }
            }

            return new List<int> { strongestEnemyIndex };
        }

        public override string GetComponentName()
        {
            return "Kingslayer";
        }

        public override string GetComponentTooltip()
        {
            return "Selects the entity with the highest combined level and health to attack.";
        }
    }
}
