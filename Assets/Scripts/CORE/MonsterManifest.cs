using System.Collections;
using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace CORE
{
    [System.Serializable]
    public class MonsterManifest
    {
        public List<Encounter> encounters = new List<Encounter>();

        public Encounter GetRandomEncounter()
        {
            return encounters[Random.Range(0, encounters.Count)];
        }
        
    }

    [System.Serializable]
    public struct Encounter
    {
        [SerializeField] private EntityScriptable[] enemies;

        public Encounter(EntityScriptable[] newEnemies)
        {
            enemies = newEnemies;
        }

        public EntityScriptable[] GetEnemies()
        {
            return enemies;
        }
    }
}