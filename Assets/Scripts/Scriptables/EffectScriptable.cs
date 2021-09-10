using UnityEngine;

namespace Scriptables
{
    public class EffectScriptable : ScriptableObject
    {
        public string effectName;
        public string effectId;
        public string effectDesc;
        public Vector2Int effectDamage;    // The X component is total damage / healing and the Y component is the total divisions for tickover.
    }
}

