using System;
using System.Collections.Generic;
using System.Linq;
using CORE;
using Items;
using SAMSARA;
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
        public List<EntityScriptable> holdingCells = new List<EntityScriptable>();
        
        // Data to be Serialized
        public List<Item> partyInventory = new List<Item>();
        public List<string> usedGroundItems = new List<string>();
        public int partyWallet = 0;
        public Vector3 lastPlayerPos = Vector3.zero;
        public Vector3 lastPlayerEulers = Vector3.zero;
        public int lastDungeonIndex = 0;
        public int lastBtnLayer = 0;
        public string playerName = "Yoel";
        public string bgmReference;
        public string[] randomBgmTracks = new string[0];

        //Persistant Data storage
        public HashSet<string> eventTriggers = new HashSet<string>();
        public HashSet<string> containerStates = new HashSet<string>();
        public Dictionary<string, bool> interactableStates = new Dictionary<string, bool>();

        public string destinationScene = "TownScene";
        public string lastTransformScene = "";

        
        
        void Start()
        {
            LoadSlim(); // Load external data, if it exists
            CreateCopies(); // Create internal copies (to avoid overwrites!)
            
            turnOrderComponent.Init(GetPlayerParty(), ref enemyField); // Create the turn queue
            
            systemStateManager.Init(this); // Start the state machine.
            
            //Add method to grab random combat track if no reference is passed.

            Samsara.Instance.MusicPlayLayered(bgmReference, TransitionType.SmoothFade, 1.5f, out bool success);
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

                if (copy.GetEntityHP().x == 0)
                    copy.deadTrigger = true;

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
                containerStates = slimData.containerStates;
                eventTriggers = slimData.eventTriggers;
                interactableStates = slimData.interactableStates;
                playerName = slimData.playerName;
                lastBtnLayer = slimData.lastButtonLayer;
                if (slimData.combatTrackRef != string.Empty)
                {
                    bgmReference = slimData.combatTrackRef;
                }
                else
                {
                    bgmReference = randomBgmTracks[UnityEngine.Random.Range(0, randomBgmTracks.Length)];
                }
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
                containerStates = containerStates,
                eventTriggers = eventTriggers,
                interactableStates = interactableStates,
                playerName = playerName,
                lastButtonLayer = lastDungeonIndex,
                combatTrackRef = ""
            };
            
            SlimComponent.Instance.PopulateAndSendSlim(slimData);
            SceneManager.LoadScene(destinationScene);
        }

        public void GoToGameOverScene()
        {
            SceneManager.LoadScene("GAME_OVER");
        }

        public EntityScriptable GetNextEntity()
        {
            TurnItem turnItem = turnOrderComponent.GetFirstInLine();

            EntityScriptable[] toSearch = (turnItem.enemyTag) ? enemyField : GetPlayerParty();

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
                {
                    if (!enemyField[i].deadTrigger)
                        toReturn.Add(i);
                }
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

        public TurnItem GetEntityTurnItem(EntityScriptable entity)
        {
            Queue<TurnItem> queue = new Queue<TurnItem>(turnOrderComponent.turnQueue);
            TurnItem toReturn = new TurnItem();

            while (queue.Count > 0)
            {
                TurnItem turnItem = queue.Dequeue();

                EntityScriptable[] toSearch = (turnItem.enemyTag) ? enemyField : GetPlayerParty();

                for (int i = 0; i < toSearch.Length; i++)
                {
                    if (toSearch[i] == null) { continue; }

                    if (toSearch[i].throwawayId == turnItem.entityId)
                        toReturn = turnItem;
                }
            }

            return toReturn;
        }

        public void AddEntityToBattle(int index, EntityScriptable entity, bool overwriteExisting = true)
        {
            if (enemyField[index] != null)
            {
                if (overwriteExisting || enemyField[index].deadTrigger)
                {
                    int tempId = enemyField[index].throwawayId;
                    turnOrderComponent.RemoveEntityFromQueue(tempId);

                    turnOrderComponent.AddEntityToTurnQueue(ref entity);
                    enemyField[index] = entity;
                }
            }
            else
            {
                turnOrderComponent.AddEntityToTurnQueue(ref entity);
                enemyField[index] = entity;
            }
        }

        public void AddEntityRandomlyToBattle(EntityScriptable entity)
        {
            var freeIndices = enemyField.Select((value, index) => new { value, index })
                      .Where(pair => pair.value == null || pair.value.deadTrigger)
                      .Select(pair => pair.index)
                      .ToList();

            if (freeIndices.Count == 0) { return; }

            int index = freeIndices[UnityEngine.Random.Range(0, freeIndices.Count)];
            if (enemyField[index] != null)
            {
                if (enemyField[index].deadTrigger)
                {
                    int tempId = enemyField[index].throwawayId;
                    turnOrderComponent.RemoveEntityFromQueue(tempId);

                    turnOrderComponent.AddEntityToTurnQueue(ref entity);
                    enemyField[index] = entity;
                }
            }
            else
            {
                turnOrderComponent.AddEntityToTurnQueue(ref entity);
                enemyField[index] = entity;
            }
        }

        public void RemoveEntityFromBattle(int throwawayId)
        {
            int index = 0;
            for (int i = 0; i < enemyField.Length; i++)
            {
                if (enemyField[i] == null) continue;
                if (enemyField[i].throwawayId == throwawayId)
                {
                    index = i;
                    break;
                }
            }
            
            int tempId = enemyField[index].throwawayId;
            turnOrderComponent.RemoveEntityFromQueue(tempId);

            enemyField[index] = null;
        }

        public void PlaceEntityInHoldingCell(EntityScriptable es)
        {
            holdingCells.Add(es);
        }

        public EntityScriptable RetrieveEntityFromHoldingCell(string entityId)
        {
            for (int i = 0; i < holdingCells.Count; i++)
            {
                if (holdingCells[i] == null) continue;
                if (holdingCells[i].entityId.Equals(entityId))
                {
                    EntityScriptable es = holdingCells[i];
                    holdingCells.RemoveAt(i);
                    return es;
                }
            }

            return null;
        }

        public EntityScriptable[] GetPlayerParty()
        {
            List<EntityScriptable> toReturn = new List<EntityScriptable>();

            for (int i = 0; i < partyField.Length; i++)
            {
                if (partyField[i] != null)
                {
                    if (partyField[i].inParty)
                        toReturn.Add(partyField[i]);
                }
            }

            int missingSlots = 4 - toReturn.Count;

            for (int i = 0; i < missingSlots; i++)
                toReturn.Add(null);

            return toReturn.ToArray();
        }
    }
}
