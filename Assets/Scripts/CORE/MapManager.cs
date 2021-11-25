using System;
using System.Collections.Generic;
using Items;
using MOVEMENT;
using SAMSARA;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CORE
{
    public class MapManager : MonoBehaviour
    {
        public GameObject slimPrefab;
        public bool lookForSlimOnStart = true;
        public SlimComponent.SlimData currentSlimData = new SlimComponent.SlimData();

        private FPSGridPlayer player;
        private bool _hasDungeonManager = false;
        public DungeonManager dungeonManager;
        public GameConfig config;
        public string mapBMGReference = "";

        public const int BattleSceneIndex = 1;

        [Space]
        
        [SerializeField] private MonsterManifest monsterManifest = new MonsterManifest();

        private void Awake()
        {
            config = Resources.Load<GameConfig>("Game Config");
            player = FindObjectOfType<FPSGridPlayer>();
            _hasDungeonManager = !(dungeonManager == null);
            if (_hasDungeonManager)
            {
                dungeonManager.SetPlayer(this, player);
            }
        }

        private void Start()
        {
            SlimComponent.Instance.ReadVolatileSlim(out SlimComponent.SlimData slimData);
            if (slimData == null || slimData.partyField.Length != 4) //Erroneous data, party should always be 4.
            {
                //Don't replace the data, it's garbo! Use defaults
            }
            else
            {
                currentSlimData.destinationScene = slimData.destinationScene;
                currentSlimData.partyField = slimData.partyField;
                currentSlimData.enemyField = slimData.enemyField;
                //If the coords are not from this scene, ignore them.
                Debug.Log($"lts: {slimData.lastTransformScene}, as: {SceneManager.GetActiveScene().name}");
                if (slimData.lastTransformScene.Equals(SceneManager.GetActiveScene().name))
                {
                    currentSlimData.playerLastPos = slimData.playerLastPos;
                    currentSlimData.playerLastEuler = slimData.playerLastEuler;
                }
                currentSlimData.inventory = slimData.inventory;
                currentSlimData.wallet = slimData.wallet;
                currentSlimData.usedGroundItems = slimData.usedGroundItems;
                currentSlimData.playerName = slimData.playerName;
            }
            
                
            //Load the dungeonIndex into the dungeonManager if one is available.
            if (_hasDungeonManager && !currentSlimData.ignoreTransformReadFlag)
            {
                dungeonManager.WarpPlayer(currentSlimData.playerLastPos, currentSlimData.playerLastEuler);
                for (int i = 0; i < currentSlimData.usedGroundItems.Count; i++)
                {
                    for (int j = 0; j < dungeonManager.groundItems.Count; j++)
                    {
                        if (dungeonManager.groundItems[j].guid.Equals(currentSlimData.usedGroundItems[i]))
                        {
                            dungeonManager.groundItems[j].triggerActive = false;
                            dungeonManager.groundItems[j].gameObject.SetActive(false);
                        }
                    }

                    for (int j = 0; j < dungeonManager.encounterTriggers.Count; j++)
                    {
                        if (dungeonManager.encounterTriggers[j].guid.Equals(currentSlimData.usedGroundItems[i]))
                        {
                            dungeonManager.encounterTriggers[j].disabled = true;
                            dungeonManager.encounterTriggers[j].gameObject.SetActive(false);
                        }
                    }
                    
                    for (int j = 0; j < dungeonManager.eventTriggers.Count; j++)
                    {
                        if (dungeonManager.eventTriggers[j].guid.Equals(currentSlimData.usedGroundItems[i]))
                        {
                            dungeonManager.eventTriggers[j].disabled = true;
                            dungeonManager.eventTriggers[j].gameObject.SetActive(false);
                        }
                    }
                }
            }

            Samsara.Instance.MusicPlayLayered(mapBMGReference, TransitionType.CrossFade, 1.2f, out bool success);
        }

        public void LoadRandomBattle()
        {
            currentSlimData.enemyField = monsterManifest.GetRandomEncounter().GetEnemies();
            SwitchSceneToBattle(SceneManager.GetActiveScene().name); //Try loading into the battle
        }

        public void SwitchSceneToBattle(string slimDestinationScene)
        {
            //Create slim here before we load the scene.

            SlimComponent.SlimData slimData = new SlimComponent.SlimData()
            {
                destinationScene = slimDestinationScene,
                lastTransformScene = SceneManager.GetActiveScene().name,
                partyField = currentSlimData.partyField,
                enemyField = currentSlimData.enemyField,
                inventory = currentSlimData.inventory,
                playerLastPos = player.transform.position,
                playerLastEuler = player.transform.rotation.eulerAngles,
                wallet = currentSlimData.wallet,
                usedGroundItems = currentSlimData.usedGroundItems,
                playerName = currentSlimData.playerName,
            };
            
            SlimComponent.Instance.PopulateAndSendSlim(slimData);
            //Perhaps trigger some script that plays a transition between the scenes using dontDestroyOnLoad?
            Debug.Log("Loading scene: "+ currentSlimData.destinationScene);
            SceneManager.LoadScene(BattleSceneIndex);
        }

        public List<Item> RequestPlayerInventory()
        {
            return currentSlimData.inventory;
        }

        public void OverwritePlayerInventory(List<Item> newItems)
        {
            List<Item> items = newItems;
            if (items.Count > config.inventorySpaces)
            {
                List<Item> temp = new List<Item>();
                for (int i = 0; i < config.inventorySpaces; i++)
                {
                    temp.Add(newItems[i]);
                }

                items = temp;
            }
            currentSlimData.inventory = items;
        }

        public int UpdateWallet(int difference)
        {
            currentSlimData.wallet += difference;

            return currentSlimData.wallet;
        }

        public void SetEnemyField(EntityScriptable[] enemies)
        {
            currentSlimData.enemyField = enemies;
        }
        
    }
}
