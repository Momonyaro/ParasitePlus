using Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.AI
{
    public abstract class MoveSelectCompBase
    {
        public string componentName;

        public abstract AbilityScriptable Evaluate(EntityScriptable entity, List<AbilityScriptable> nodeAbilities);

        public virtual string GetComponentName()
        {
            return "NOT IMPLEMENTED [ERR]";
        }

        public virtual string GetComponentTooltip()
        {
            return "NOT IMPLEMENTED [ERR]";
        }
    }

    public class MoveSelectCompRandom : MoveSelectCompBase
    {
        public override AbilityScriptable Evaluate(EntityScriptable entity, List<AbilityScriptable> nodeAbilities)
        {
            List<AbilityScriptable> abilities = new List<AbilityScriptable>(entity.GetEntityAbilities());
            abilities.AddRange(nodeAbilities);
            abilities.Add(entity.defaultAttack);

            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].abilityCooldown.x = Mathf.FloorToInt(Mathf.Clamp(abilities[i].abilityCooldown.x - 1, 0, 100));
            }
            
            for (int i = 0; i < abilities.Count; i++)
            {
                int randomTest = Random.Range(0, abilities.Count);
                if (abilities[randomTest].abilityCosts.x < entity.GetEntityAP().x && abilities[randomTest].abilityCooldown.x == 0)
                {
                    return abilities[randomTest];
                }
            }

            return entity.defaultAttack;
        }

        public override string GetComponentName()
        {
            return "Random";
        }

        public override string GetComponentTooltip()
        {
            return "Selects a skill at random, if entity AP is too low we default to the normal attack. (Normal attacks are also included in the random pick for good measure.)";
        }
    }

    public class MoveSelectCompBurst : MoveSelectCompBase
    {
        public override AbilityScriptable Evaluate(EntityScriptable entity, List<AbilityScriptable> nodeAbilities)
        {
            List<AbilityScriptable> abilities = new List<AbilityScriptable>(entity.GetEntityAbilities());
            abilities.AddRange(nodeAbilities);
            AbilityScriptable mostExpensive = entity.defaultAttack;
            for (int i = 0; i < abilities.Count; i++)
            {
                if ((abilities[i].abilityCosts.x < entity.GetEntityAP().x) && abilities[i].abilityCosts.x >= mostExpensive.abilityCosts.x  && abilities[i].abilityCooldown.x == 0)
                {
                    mostExpensive = abilities[i];
                }
            }

            return mostExpensive;
        }

        public override string GetComponentName()
        {
            return "Burst";
        }

        public override string GetComponentTooltip()
        {
            return "Selects the most expensive skill that the entity has AP for. When we don't have any more AP for skills, default to the normal attack.";
        }
    }
}
