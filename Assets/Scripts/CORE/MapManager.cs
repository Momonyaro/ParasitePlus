using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using MOVEMENT;
using SAMSARA;
using Scriptables;
using UI;
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
        public DungeonManager dungeonManager;
        public GameConfig config;
        public string mapBMGReference = "";
        public bool writeSaveOnLoad = true;

        public const int BattleSceneIndex = 1;

        public bool HasDungeonManager => dungeonManager != null;

        [Space]
        
        [SerializeField] private MonsterManifest monsterManifest = new MonsterManifest();

        private void Awake()
        {
            config = Resources.Load<GameConfig>("Game Config");
            player = FindObjectOfType<FPSGridPlayer>();
            if (HasDungeonManager)
            {
                dungeonManager.SetPlayer(this, player);
            }

            SlimComponent.Instance.ReadVolatileSlim(out SlimComponent.SlimData slimData);

            //Save to external file
            if (writeSaveOnLoad)
                SaveUtility.WriteToDisk(slimData);

            if (slimData == null || slimData.partyField.Length != 4) //Erroneous data, party should always be 4.
            {
                //Don't replace the data, it's garbo! Use defaults
                for (int i = 0; i < currentSlimData.partyField.Length; i++)
                {
                    if (currentSlimData.partyField[i] != null)
                    {
                        currentSlimData.partyField[i] = currentSlimData.partyField[i].Copy();
                    }
                }
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
                currentSlimData.containerStates = slimData.containerStates;
                currentSlimData.eventTriggers = slimData.eventTriggers;
                currentSlimData.interactableStates = slimData.interactableStates;
                currentSlimData.playerName = slimData.playerName;
                currentSlimData.loadSceneVariable = slimData.loadSceneVariable;
                currentSlimData.lastButtonLayer = slimData.lastButtonLayer;
            }
            
                
            //Load the dungeonIndex into the dungeonManager if one is available.
            if (HasDungeonManager && !currentSlimData.ignoreTransformReadFlag)
            {
                dungeonManager.WarpPlayer(currentSlimData.playerLastPos, currentSlimData.playerLastEuler);
                for (int i = 0; i < currentSlimData.containerStates.Count; i++)
                {
                    for (int j = 0; j < dungeonManager.groundItems.Count; j++)
                    {
                        if (currentSlimData.containerStates.Contains(dungeonManager.groundItems[j].guid))
                        {
                            dungeonManager.groundItems[j].triggerActive = false;
                            dungeonManager.groundItems[j].lockTrigger = true;
                            if (dungeonManager.groundItems[j].hasAnimator)
                                dungeonManager.groundItems[j].objAnimator.SetBool(dungeonManager.groundItems[j].animParamName, true);
                        }
                    }

                    for (int j = 0; j < dungeonManager.encounterTriggers.Count; j++)
                    {
                        if (currentSlimData.eventTriggers.Contains(dungeonManager.encounterTriggers[j].guid))
                        {
                            dungeonManager.encounterTriggers[j].disabled = true;
                            dungeonManager.encounterTriggers[j].gameObject.SetActive(false);
                        }
                    }
                    
                    for (int j = 0; j < dungeonManager.eventTriggers.Count; j++)
                    {
                        if (currentSlimData.eventTriggers.Contains(dungeonManager.eventTriggers[j].guid))
                        {
                            dungeonManager.eventTriggers[j].disabled = true;
                            dungeonManager.eventTriggers[j].gameObject.SetActive(false);
                        }
                    }

                    for (int j = 0; j < dungeonManager.doorInteractables.Count; j++)
                    {
                        if (currentSlimData.interactableStates.ContainsKey(dungeonManager.doorInteractables[j].guid))
                        {
                            dungeonManager.doorInteractables[j].locked = currentSlimData.interactableStates[dungeonManager.doorInteractables[j].guid];
                        }
                    }

                    for (int j = 0; j < dungeonManager.mapInteractables.Count; j++)
                    {
                        if (currentSlimData.interactableStates.ContainsKey(dungeonManager.mapInteractables[j].guid))
                        {
                            dungeonManager.mapInteractables[j].activeState = currentSlimData.interactableStates[dungeonManager.mapInteractables[j].guid];
                            dungeonManager.mapInteractables[j].UpdateLightStates();
                        }
                    }
                }

                for (int i = 0; i < dungeonManager.sceneLoadVariables.Count; i++)
                {
                    if (dungeonManager.sceneLoadVariables[i].reference.Equals(currentSlimData.loadSceneVariable))
                    {
                        dungeonManager.WarpPlayer(dungeonManager.sceneLoadVariables[i].playerPosition, dungeonManager.sceneLoadVariables[i].playerRotation);
                        break;
                    }
                }
            }

            if (!Samsara.Instance.GetMusicPlaying(out bool success).Equals(mapBMGReference))
                Samsara.Instance.MusicPlayLayered(mapBMGReference, TransitionType.CrossFade, 1.2f, out success);
        }

        public void GoToNewScene(string destination, string loadSceneVar)
        {
            currentSlimData.destinationScene = destination;
            currentSlimData.loadSceneVariable = loadSceneVar;

            StartCoroutine(WaitForSceneTransition(destination));

            IEnumerator WaitForSceneTransition(string newSceneRef)
            {
                SlimComponent.Instance.PopulateAndSendSlim(currentSlimData);
                FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();

                player.AddLock("MAP_MANAGER");

                fadeToBlackImage.FadeToBlack(0.3f, .8f);

                while (!fadeToBlackImage.screenBlack) { yield return null; }

                SwitchScene(newSceneRef, false);

                yield break;
            }
        }

        

        public void LoadRandomBattle()
        {
            currentSlimData.enemyField = monsterManifest.GetRandomEncounter().GetEnemies();
            SwitchSceneToBattle(SceneManager.GetActiveScene().name, String.Empty); //Try loading into the battle
        }

        public void SwitchSceneToBattle(string slimDestinationScene, string musicRef)
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
                containerStates = currentSlimData.containerStates,
                eventTriggers = currentSlimData.eventTriggers,
                interactableStates = currentSlimData.interactableStates,
                playerName = currentSlimData.playerName,
                loadSceneVariable = currentSlimData.loadSceneVariable,
                lastButtonLayer = currentSlimData.lastButtonLayer,
                combatTrackRef = musicRef
            };
            
            SlimComponent.Instance.PopulateAndSendSlim(slimData);
            //Perhaps trigger some script that plays a transition between the scenes using dontDestroyOnLoad?
            Debug.Log("Loading scene: "+ currentSlimData.destinationScene);
            SceneManager.LoadScene(BattleSceneIndex);
        }

        public void SwitchScene(string destinationReference, bool parseDestination)
        {
            //Create slim here before we load the scene.

            SlimComponent.SlimData slimData = new SlimComponent.SlimData()
            {
                destinationScene = destinationReference,
                lastTransformScene = SceneManager.GetActiveScene().name,
                partyField = currentSlimData.partyField,
                enemyField = currentSlimData.enemyField,
                inventory = currentSlimData.inventory,
                playerLastPos = player.transform.position,
                playerLastEuler = player.transform.rotation.eulerAngles,
                wallet = currentSlimData.wallet,
                containerStates = currentSlimData.containerStates,
                eventTriggers = currentSlimData.eventTriggers,
                interactableStates = currentSlimData.interactableStates,
                playerName = currentSlimData.playerName,
                loadSceneVariable = currentSlimData.loadSceneVariable,
                lastButtonLayer = currentSlimData.lastButtonLayer
            };

            string destination = destinationReference;
            if (parseDestination)
            {
                SceneParser.ParseSceneChange(destinationReference, out string slimDestination, out destination);
                currentSlimData.destinationScene = slimDestination;
            }
            
            SlimComponent.Instance.PopulateAndSendSlim(slimData);
            //Perhaps trigger some script that plays a transition between the scenes using dontDestroyOnLoad?
            Debug.Log("Loading scene: "+ destination);
            SceneManager.LoadScene(destination);

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

        public void OverwritePartyState(bool[] partyStates)
        {
            for (int i = 0; i < currentSlimData.partyField.Length; i++)
            {
                if (currentSlimData.partyField[i] != null)
                {
                    currentSlimData.partyField[i].inParty = partyStates[i];
                }
            }
        }

        public void WritePersistantState(string key, bool state)
        {
            if (currentSlimData.interactableStates.ContainsKey(key))
            {
                currentSlimData.interactableStates[key] = state;
            }
            else
                currentSlimData.interactableStates.Add(key, state);
        }

        public bool PersistantStateExists(string key)
        {
            return (currentSlimData.interactableStates.ContainsKey(key));
        }

        public bool GetPersistantState(string key)
        {
            if (!currentSlimData.interactableStates.ContainsKey(key))
                return false;

            return currentSlimData.interactableStates[key];
        }
        
    }
}
