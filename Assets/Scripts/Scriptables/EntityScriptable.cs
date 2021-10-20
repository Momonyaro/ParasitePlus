using System;
using System.Collections.Generic;
using BattleSystem;
using BattleSystem.AI;
using CORE;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "EntityScriptable", menuName = "ScriptableObjects/Entities", order = 1)]
    public class EntityScriptable : ScriptableObject
    {
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        /*                                                                   */
        /* PLEASE REMEMBER TO COPY THE SCRIPTABLES AS TO NOT OVERWRITE DATA! */
        /*                   it would be rather painful...                   */
        /*                                                                   */
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * Sebastian */


        public string entityName;
        public int entityLevel;
        public string entityId;
        public int entitySpeed;
        public int throwawayId;          //This is a thing because if we have multiple versions of the same entity, it will fetch the first one.
        [SerializeField]
        private Vector2Int healthPts;    //The X component is the current value and the Y component is the max value
        [SerializeField]
        private Vector2Int actionPts;    //The X component is the current value and the Y component is the max value

        public float[] weaknesses = new float[4]  // Magic, Blunt, Pierce, Slash
        {
            1, 1, 1, 1
        };
        
        public int entityXp;            // For enemies, this value will be the xp reward for beating it.a
        public int entityXpThreshold;   // Going over this threshold registers as a levelup.
        public AbilityScriptable defaultAttack;
        public EntityScriptable lastAttacker;
        [SerializeField] private EntityAIComponent entityAI = new EntityAIComponent();
        public bool deadTrigger;

        public enum STAT
        {
            STRENGTH  = 0,  //DECIDES DAMAGE FOR MELEE WEAPONS
            DEXTERITY = 1,  //DECIDES DAMAGE FOR RANGED WEAPONS
            ENDURANCE = 2,  //THIS SCALES YOUR HEALTH
            AGILITY   = 3,  //THIS HELPS WITH DODGE CHANCE & ENTRY TURN ORDER
            TOLERANCE = 4,  //THIS SLIGHTLY INCREASES RESISTANCES
            LUCK      = 5   //DECIDES YOUR DODGE CHANCE
        }

        private const int STAT_COUNT = 6;
        [SerializeField] private int[] stats = new int[STAT_COUNT];

        //Here we should keep track of the abilities of the entity. Should they always have a "default attack"?
        //It would literally be an array of Ability scriptable.

        public AbilityScriptable[]  abilityScriptables;
        public EffectScriptable[]   entityEffectBuffer;      //Here is where all current effects on the entity are updated

        private GameConfig config;


        // There should perhaps not be any code in the scriptable, Handle it in the "Battle Director" instead. -Sebastian

        private void Awake()
        {
            config = Resources.Load<GameConfig>("Game Config");
        }

        private void Start()
        {
            if (healthPts.x <= 0) deadTrigger = true; // Why does this break if used in Awake?
        }

        #region GETTERS & SETTERS

        public int[] GetEntityStats()
        {
            return stats;
        }

        public void SetEntityStats(int[] newStats)
        {
            for (int i = 0; i < STAT_COUNT; i++)
            {
                newStats[i] = Mathf.Min(newStats[i], config.statCap);
            }

            stats = newStats;
        }


        public Vector2Int GetEntityHP()
        {
            return healthPts;
        }

        public void SetEntityHP(Vector2Int newHealth)
        {
            healthPts = newHealth;

            healthPts.y = Mathf.Min(healthPts.y, config.healthCap);
            healthPts.x = Mathf.Min(healthPts.x, healthPts.y);

            if (healthPts.x <= 0) deadTrigger = true;
        }


        public Vector2Int GetEntityAP()
        {
            return actionPts;
        }

        public void SetEntityAP(Vector2Int newActionPoints)
        {
            actionPts = newActionPoints;
        }

    
        public AbilityScriptable[] GetEntityAbilities()
        {
            var toReturn = new List<AbilityScriptable>();

            for (int i = 0; i < abilityScriptables.Length; i++)
            {
                if (abilityScriptables[i].abilityLevelReq <= entityLevel)
                    toReturn.Add(abilityScriptables[i]);
            }
        
            return toReturn.ToArray();
        }
        
        public AbilityScriptable[] GetAllEntityAbilities()
        {
            return abilityScriptables;
        }

        public AbilityScriptable GetAbilityByID(string abiltyId)
        {
            for (int i = 0; i < abilityScriptables.Length; i++)
            {
                if (abilityScriptables[i].abilityId.Equals(abiltyId))
                    return abilityScriptables[i];
            }

            return null;
        }

        public void SetEntityAbilities(AbilityScriptable[] abilities)
        {
            abilityScriptables = abilities;
        }

        public void AddXpToEntity(int newXp)
        {
            int xp = entityXp + newXp;

            while (xp >= entityXpThreshold) // DING! You've levelled up.
            {
                entityLevel++;
                xp -= entityXpThreshold; // Only carry-over remaining.
                
                CalculateNewXpThreshold();
            }
            
            entityXp = xp; // Write remaining xp to buffer;
        }

        public void CalculateNewXpThreshold()
        {
            // This is where we will have out xp curve calculation.
            
            // Guessing something exponential.
            
            entityXpThreshold = entityLevel * 14;
        }

        public EntityAIComponent GetAIComponent()
        {
            return entityAI;
        }

        public void OverwriteAIComponent(EntityAIComponent newAIComp)
        {
            entityAI = newAIComp;
            for (int i = 0; i < entityAI.personalityNodes.Count; i++)
            {
                entityAI.personalityNodes[i].hasReadFirstLoop = false;
                entityAI.personalityNodes[i].onFirstLoopInterjects =
                    new List<InterjectBase>(newAIComp.personalityNodes[i].onFirstLoopInterjects);
                entityAI.personalityNodes[i].targetComp = newAIComp.personalityNodes[i].targetComp;
                entityAI.personalityNodes[i].moveSelect = newAIComp.personalityNodes[i].moveSelect;
            }
        }

        public EntityScriptable Copy()
        {
            EntityScriptable es = CreateInstance<EntityScriptable>();
            es.abilityScriptables = abilityScriptables;
            es.defaultAttack = defaultAttack;
            es.actionPts = actionPts;
            es.entityEffectBuffer = entityEffectBuffer;
            es.entityId = entityId;
            es.entityName = entityName;
            es.entitySpeed = entitySpeed;
            es.healthPts = healthPts;
            es.stats = stats;
            es.entityLevel = entityLevel;
            es.entityXp = entityXp;
            es.entityXpThreshold = entityXpThreshold;
            es.OverwriteAIComponent(entityAI);
            es.weaknesses = weaknesses;

            return es;
        }

        #endregion
    }
}
