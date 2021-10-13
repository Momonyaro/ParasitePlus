using Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.AI
{
    [System.Serializable]
    public class EntityAIComponent
    {
        public int randID = -1;

        private List<MoveSelectCompBase> allMoveSelectComponents = new List<MoveSelectCompBase>()
        {
            new MoveSelectCompRandom(),
            new MoveSelectCompBurst(),
        };

        private List<TargetCompBase> allTargetingComponents = new List<TargetCompBase>()
        { 
            new TargetCompRandom(),
            new TargetCompBully(),
            new TargetCompVengeful(),
            new TargetCompKingslayer(),
        };

        public List<MoveSelectCompBase> AllMoveSelect => allMoveSelectComponents;
        public List<TargetCompBase> AllTargeting => allTargetingComponents;

        public List<PersonalityNode> personalityNodes = new List<PersonalityNode>();


        public PersonalityNode GetCurrentPersonalityNode(EntityScriptable entity, int currentTurn)
        {
            Debug.Log(personalityNodes.Count);
            PersonalityNode toReturn = personalityNodes[0];
            for (int i = 0; i < personalityNodes.Count; i++)
            {
                PersonalityNode current = personalityNodes[i];
                bool healthCheck = (current.healthPercentLessThan >= (entity.GetEntityHP().x / (float)entity.GetEntityHP().y) );
                bool turnCheck   = (current.roundsPassed <= currentTurn);

                if (healthCheck && turnCheck)
                    toReturn = current;
            }

            return toReturn;
        }

        public void SetAIOwner(EntityScriptable entity)
        {
            for (int i = 0; i < personalityNodes.Count; i++)
            {
                personalityNodes[i].targetComp.SetOwner(entity);
            }
        }

        public EntityAIComponent()
        {
            if (personalityNodes.Count == 0)
            {
                personalityNodes = new List<PersonalityNode>
                {
                    new PersonalityNode()
                    {
                        healthPercentLessThan = 1.0f,
                        roundsPassed = 0,
                        moveSelect = allMoveSelectComponents[0],
                        targetComp = allTargetingComponents[0],
                    }
                };
            }
        }

        [System.Serializable]
        public class PersonalityNode
        {
            [Header("Node Requirements")]
            [Range(0, 1)] public float healthPercentLessThan = 1.0f;
            public int roundsPassed = 0;
            public string trigger;
            private bool triggered = false;

            [Header("Node Interjects")]
            [HideInInspector] [SerializeReference] public List<InterjectBase> onFirstLoopInterjects = new List<InterjectBase>();

            public bool hasReadFirstLoop = false;


            [Header("Node Personality Components")]
            [SerializeReference] public MoveSelectCompBase moveSelect;
            [SerializeReference] public TargetCompBase targetComp;

            public void OverwriteMoveSelect(MoveSelectCompBase newMoveSelect)
            {
                moveSelect = newMoveSelect;
            }

            public void OverwriteTargeting(TargetCompBase newTargeting)
            {
                targetComp = newTargeting;
            }
        }
    }
}
