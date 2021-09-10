using System;
using System.Collections.Generic;
using CORE;
using Items;
using Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleSystem
{
    public class BattleSystemCore : MonoBehaviour
    {
        ////COMPONENTS////
        public BattleSystemTurnOrder turnOrderComponent = new BattleSystemTurnOrder(); // Manages turn queue, gives entities unique IDs
        public BattleSystemStateManager systemStateManager = new BattleSystemStateManager(); // Manages system states.
        //////////////////
        
        [Space()]
        [Space()]
        
        // The entity data
        public EntityScriptable[] partyField = new EntityScriptable[4]; // With new system, ignore fourth item
        public EntityScriptable[] enemyField = new EntityScriptable[5]; // With new system, ignore items 6 - 9
        
        // Data to be Serialized
        public List<Item> partyInventory = new List<Item>();
        public List<string> usedGroundItems = new List<string>();
        public int partyWallet = 0;
        public Vector3 lastPlayerPos = Vector3.zero;
        public Vector3 lastPlayerEulers = Vector3.zero;
        public int lastDungeonIndex = 0;
        public string playerName = "Yoel";
        
        public string destinationScene = "TownScene";
        public string lastTransformScene = "";

        
        
        void Start()
        {
            LoadSlim(); // Load external data, if it exists
            CreateCopies(); // Create internal copies (to avoid overwrites!)
            
            turnOrderComponent.Init(ref partyField, ref enemyField); // Create the turn queue
            
            systemStateManager.Init(this); // Start the state machine.
        }

        // Update is called once per frame
        void Update()
        {
            systemStateManager.Update();
        }

        private void OnDestroy()
        {
            systemStateManager.Disassemble();
        }

        private void OnApplicationQuit()
        {
            systemStateManager.Disassemble();
        }

        void CreateCopies()
        {
            for (int i = 0; i < partyField.Length; i++)
            {
                if (partyField[i] == null) continue;
                EntityScriptable copy = partyField[i].Copy();

                partyField[i] = copy;
            }
            
            for (int i = 0; i < enemyField.Length; i++)
            {
                if (enemyField[i] == null) continue;
                EntityScriptable copy = enemyField[i].Copy();

                enemyField[i] = copy;
            }
        }
        
        void LoadSlim()
        { 
            SlimComponent.Instance.ReadVolatileSlim(out SlimComponent.SlimData slimData);
            if (slimData == null || slimData.partyField.Length != 4) //Erroneous data, party should always be 4.
            {
                //Don't replace the data, it's garbo!
            }
            else
            {
                destinationScene = slimData.destinationScene;
                lastTransformScene = slimData.lastTransformScene;
                partyField = slimData.partyField;
                enemyField = slimData.enemyField;
                partyInventory = slimData.inventory;
                partyWallet = slimData.wallet;
                lastPlayerPos = slimData.playerLastPos;
                lastPlayerEulers = slimData.playerLastEuler;
                lastDungeonIndex = slimData.lastDungeonIndex;
                usedGroundItems = slimData.usedGroundItems;
                playerName = slimData.playerName;
            }
        }

        public void GoToDestinationScene()
        {
            SlimComponent.SlimData slimData = new SlimComponent.SlimData()
            {
                destinationScene = destinationScene,
                lastTransformScene = lastTransformScene,
                partyField = partyField,
                enemyField = enemyField,
                inventory = partyInventory,
                wallet = partyWallet,
                playerLastPos = lastPlayerPos,
                playerLastEuler = lastPlayerEulers,
                lastDungeonIndex = lastDungeonIndex,
                usedGroundItems = usedGroundItems,
                playerName = playerName,
            };
            
            SlimComponent.Instance.PopulateAndSendSlim(slimData);
            SceneManager.LoadScene(destinationScene);
        }

        public EntityScriptable GetNextEntity()
        {
            TurnItem turnItem = turnOrderComponent.GetFirstInLine();

            EntityScriptable[] toSearch = (turnItem.enemyTag) ? enemyField : partyField;

            for (int i = 0; i < toSearch.Length; i++)
            {
                if (toSearch[i] == null) { continue; }

                if (toSearch[i].throwawayId == turnItem.entityId)
                    return toSearch[i];
            }

            return null;
        }

        public List<int> GetIndicesInUseByEnemies()
        {
            List<int> toReturn = new List<int>();
            for (int i = 0; i < enemyField.Length; i++)
            {
                if (enemyField[i] != null)
                    toReturn.Add(i);
            }

            return toReturn;
        }

        public EntityScriptable[] GetTurnQueueAsEntities()
        {
            Queue<TurnItem> queue = new Queue<TurnItem>(turnOrderComponent.turnQueue);
            List<EntityScriptable> toReturn = new List<EntityScriptable>();

            while (queue.Count > 0)
            {
                TurnItem turnItem = queue.Dequeue();

                EntityScriptable[] toSearch = (turnItem.enemyTag) ? enemyField : partyField;

                for (int i = 0; i < toSearch.Length; i++)
                {
                    if (toSearch[i] == null) { continue; }

                    if (toSearch[i].throwawayId == turnItem.entityId)
                        toReturn.Add(toSearch[i]);
                }
            }

            return toReturn.ToArray();
        }
    }
}
