using System;
using System.Collections.Generic;
using Items;
using Scriptables;
using UnityEngine;

namespace CORE
{
    public class SlimComponent : MonoBehaviour
    {

        #region singletonStuff
        
        static SlimComponent sInstance;

        public static SlimComponent Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject go = new GameObject("Slim Shuttle");
                    DontDestroyOnLoad(go);
                    sInstance = go.AddComponent<SlimComponent>();
                }

                return sInstance;
            }
        }

        #endregion

        [SerializeField] private SlimData internalSlimData = new SlimData();

        public void PopulateAndSendSlim(SlimData slimData)
        {
            sInstance.internalSlimData = slimData;
        }

        public void ReadVolatileSlim(out SlimData slimData)
        {
            sInstance.internalSlimData.PrintPersistantData();

            slimData = sInstance.internalSlimData;
            Destroy(sInstance.gameObject);
        }

        public string ReadNonVolatilePlayerName => internalSlimData.playerName;
        public string ReadNonVolatileDesination => internalSlimData.destinationScene;

        public void SetNonVolatileDestination(string destination)
        {
            internalSlimData.destinationScene = destination;
        }


        [System.Serializable]
        public class SlimData
        {
            public string destinationScene = "";
            public string lastTransformScene = "";
            public string playerName = "Yoel";
            public EntityScriptable[] partyField = new EntityScriptable[0];
            public EntityScriptable[] enemyField = new EntityScriptable[0];
            public List<Item> inventory = new List<Item>();
            public int wallet = 0;
            public bool ignoreTransformReadFlag = false;
            public Vector3 playerLastPos = Vector3.zero;
            public Vector3 playerLastEuler = Vector3.zero;
            public string loadSceneVariable = "";
            public int lastDungeonIndex = 0; // Default to first piece of the dungeon

            //Persistant Data storage
            public HashSet<string> eventTriggers = new HashSet<string>();
            public HashSet<string> containerStates = new HashSet<string>();
            public Dictionary<string, bool> interactableStates = new Dictionary<string, bool>();

            public void PrintPersistantData()
            {
                string interactableOutput = "[INTERACTABLES] ::--::";
                foreach (string key in interactableStates.Keys)
                {
                    interactableOutput += $"\n[{key}: {interactableStates[key]}]";
                }
                Debug.Log(interactableOutput);

                string eventTriggerOutput = "[EVENTS] ::--::";
                foreach (string key in eventTriggers)
                {
                    eventTriggerOutput += $"\n[{key}]";
                }
                Debug.Log(eventTriggerOutput);

                string containersOutput = "[CONTAINERS] ::--::";
                foreach (string key in eventTriggers)
                {
                    containersOutput += $"\n[{key}]";
                }
                Debug.Log(containersOutput);
            }
        }
    }
}
