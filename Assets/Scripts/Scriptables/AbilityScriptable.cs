using BATTLE;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "Entity Ability", menuName = "Abilities/Entity Ability", order = 0)]
    public class AbilityScriptable : ScriptableObject
    {
        public string abilityName;
        public string abilityId;            // The ID is used for referring to the ability in code, also perhaps the icon.
        [TextArea(2, 5)]
        public string abilityDesc;
        [Space]
        public string abilityEffectRef;
        public bool targetAll;
        public bool targetFriendlies;
        public int abilityLevelReq; //This is the level the entity needs to be to be able to use the ability.
        public float levelScalingDamage = 1.0f;
        [Range(0, 1)] public float abilityCritChance;
        [Range(0, 1)] public float abilityMissChance;
        public Vector2Int abilityCosts;     // The X component is AP cost and the Y component is HP cost.
        public Vector2Int abilityDamage;    // The X component is total damage / healing and the Y component is the ap drain to the enemy.

        public float[] damageType = new float[4] // Magic, Blunt, Pierce, Slash
        {
            1, 1, 1, 1
        };

        public EffectScriptable[] abilityEffects;
    
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
        /*                                                                   */
        /* THIS BASE CLASS IS MADE TO MAINLY HOLD DATA, CREATE CLASSES THAT  */
        /*            INHERIT THIS INSTEAD TO ADD MOVE LOGIC                 */
        /*                                                                   */
        /* * * * * * * * * * * * * * * * * * * * * * * * * * * * * Sebastian */

        
        
        //DEPRECATED, DO NOT USE
        public bool Execute(EntityScriptable active, EntityScriptable[] targets, Transform[] targetTransforms, int selectorIndex, BattleManager battleManager) { return true; }
        public void CreateDamageNum(int damage, Transform target, float numHeightOffset, bool showStripe) { }
    }
}
