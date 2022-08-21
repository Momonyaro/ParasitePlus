using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using MapTriggers;
using MOVEMENT;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace CORE
{
    public class DungeonManager : MonoBehaviour
    {
        
        // The main task of this script will be handling what part of the map the player is currently on and the loading
        // between areas. We will also have to sync this with the SLIMs so that we load back to the correct map part.
        
        // Also! Remember to add the player's position to the SLIMs so that we know exactly where to place the player when 
        // we load back into the map!

        public List<GroundItem> groundItems = new List<GroundItem>();
        public List<DoorInteractable> doorInteractables = new List<DoorInteractable>();
        public List<MapInteractable> mapInteractables = new List<MapInteractable>();
        public bool enterOnUnlock = true;
        public List<EncounterTrigger> encounterTriggers = new List<EncounterTrigger>();
        public List<EventTrigger> eventTriggers = new List<EventTrigger>();
        public List<SceneLoadVariable> sceneLoadVariables = new List<SceneLoadVariable>();
        public float encounterProgress = 0; // When this reaches 1, start a random encounter.
        private bool startedEncounter = false;
        public bool randomEncounters = true;
        public float encounterProgressRange = 0.1f; // Add a random amount that's from 0 - the range max
        
        private FPSGridPlayer currentPlayer;
        private MapManager mapManager;

        private void Start()
        {
            currentPlayer.onSuccessfulMove.AddListener(IncrementEncounterProgress);
        }

        private void Update()
        {
            if (currentPlayer.lockPlayer) return;
            
            for (int i = 0; i < encounterTriggers.Count; i++)
            {
                if (encounterTriggers[i].triggerActive)
                {
                    StartCoroutine(TransitionToBattle(encounterTriggers[i]));
                }
            }
            for (int i = 0; i < eventTriggers.Count; i++)
            {
                if (eventTriggers[i].triggerActive)
                {
                    eventTriggers[i].triggerActive = false;
                    eventTriggers[i].disabled = true;
                    mapManager.currentSlimData.eventTriggers.Add(eventTriggers[i].guid);
                    eventTriggers[i].PlayEvent();
                }
            }

            if (encounterProgress >= 1 && !startedEncounter)
            {
                startedEncounter = true;
                currentPlayer.lockPlayer = true;
                StartCoroutine(TransitionToRandomBattle());
            }
        }

        public void TransitionToBattleFromTrigger(EncounterTrigger trigger)
        {
            StartCoroutine(TransitionToBattle(trigger));
        }

        private IEnumerator TransitionToBattle(EncounterTrigger trigger)
        {
            mapManager.SetEnemyField(trigger.enemyRoster);
            currentPlayer.lockPlayer = true;
            trigger.triggerActive = false;
            mapManager.currentSlimData.eventTriggers.Add(trigger.guid);
            
            FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();
            
            fadeToBlackImage.FadeToBlack(0.3f, .5f, true);
            
            while (!fadeToBlackImage.finished) { yield return null; }
            
            mapManager.SwitchSceneToBattle(trigger.postBattleSceneName);
            
            yield break;
        }
        
        private IEnumerator TransitionToRandomBattle()
        {
            FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();
            
            fadeToBlackImage.FadeToBlack(0.3f, .5f, true);
            
            while (!fadeToBlackImage.finished) { yield return null; }
            
            mapManager.LoadRandomBattle();
            
            yield break;
        }

        public void SetPlayer(MapManager manager, FPSGridPlayer player)
        {
            this.currentPlayer = player;
            mapManager = manager;
        }

        public FPSGridPlayer GetPlayer()
        {
            return currentPlayer;
        }

        public void CheckToEnterDoorTrigger()
        {
            for (int i = 0; i < doorInteractables.Count; i++)
            {
                if (doorInteractables[i] == null) continue;
                
                //Add new doors for the FPS sections
                if (doorInteractables[i].triggerActive)
                {
                    Debug.Log(doorInteractables[i].gameObject.name);
                    if (doorInteractables[i].locked)
                    {
                        DoorInteractable locked = doorInteractables[i];

                        bool hasKey = CheckValidKeyForItemLock(locked.keyItemID, mapManager.RequestPlayerInventory(),
                            locked.singleUseKey);

                        doorInteractables[i].locked = !hasKey;
                        
                        if (hasKey)
                        {
                            ItemDatabase database = Resources.Load<ItemDatabase>("Item Database");
                            Item keyItem = database.GetItemUsingGuid(locked.keyItemID);

                            string itemKeyword = $"<color=yellow>[{keyItem.name}]</color>";
                            string treatedMsg = locked.onUnlockMessage.Replace("$ITEM", itemKeyword);
                            InfoPrompt.Instance.CreatePrompt(new []{treatedMsg});
                            if (!enterOnUnlock) break;
                        }
                        else
                        {
                            InfoPrompt.Instance.CreatePrompt(new []{locked.isLockedMessage});
                            break;
                        }
                    }
                    
                    Debug.Log("Walking through Door at pos:" + doorInteractables[i].transform.position);
                    if (doorInteractables[i].playSound)
                        SAMSARA.Samsara.Instance.PlaySFXRandomTrack(doorInteractables[i].sfxReference, out bool success);

                    if (doorInteractables[i].goToScene)
                    {
                        mapManager.currentSlimData.loadSceneVariable = doorInteractables[i].loadSceneVariable;
                        StartCoroutine(WaitForSceneTransition(doorInteractables[i].sceneName));
                    }
                    else
                    {
                        StartCoroutine(WaitForDoorTranstion(i, (doorInteractables[i].playSound && doorInteractables[i].sfxReference.Equals("_doorOpen"))));
                    }
                    break;
                }
            }
        }

        public DoorInteractable CheckForActiveDoor()
        {
            for (int i = 0; i < doorInteractables.Count; i++)
            {
                if (doorInteractables[i] == null) continue;

                if (doorInteractables[i].triggerActive)
                {
                    return doorInteractables[i];
                }
            }

            return null;
        }

        public void CheckToUseInteractable()
        {
            MapInteractable active = CheckForInteractable();

            if (active == null) return;

            active.PlayEvent();
            active.triggerActive = false;

            if (mapManager.currentSlimData.interactableStates.ContainsKey(active.guid))
                mapManager.currentSlimData.interactableStates[active.guid] = active.activeState;
            else
                mapManager.currentSlimData.interactableStates.Add(active.guid, active.activeState);
            
            InfoPrompt.Instance.CreatePrompt(new[] { active.onUseMsg });
        }

        public MapInteractable CheckForInteractable()
        {
            for (int i = 0; i < mapInteractables.Count; i++)
            {
                if (mapInteractables[i] == null) continue;

                if (mapInteractables[i].triggerActive)
                {
                    return mapInteractables[i];
                }
            }

            return null;
        }

        private IEnumerator WaitForDoorTranstion(int currentIndex, bool playSound)
        {
            FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();
            bool skipPlayerLock = currentPlayer.lockPlayer;

            if (!skipPlayerLock)
                currentPlayer.lockPlayer = true;
            
            fadeToBlackImage.FadeToBlack(0.3f, .8f);
            
            while (!fadeToBlackImage.screenBlack) { yield return null; }
            
            Vector3 camEulers = currentPlayer.transform.rotation.eulerAngles;
            WarpPlayer(doorInteractables[currentIndex].warpDest, camEulers);
            
            while (!fadeToBlackImage.finished) { yield return null; }
            
            if (!skipPlayerLock)
                currentPlayer.lockPlayer = false;

            if (playSound)
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_doorClose", out bool success);

            yield break;
        }

        private IEnumerator WaitForSceneTransition(string newSceneRef)
        {
            SlimComponent.Instance.PopulateAndSendSlim(mapManager.currentSlimData);
            FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();
            bool skipPlayerLock = currentPlayer.lockPlayer;

            if (!skipPlayerLock)
                currentPlayer.lockPlayer = true;
            
            fadeToBlackImage.FadeToBlack(0.3f, .8f);
            
            while (!fadeToBlackImage.screenBlack) { yield return null; }
            
            mapManager.SwitchScene(newSceneRef, false);
            
            while (!fadeToBlackImage.finished) { yield return null; }
            
            if (!skipPlayerLock)
                currentPlayer.lockPlayer = false;
            
            yield break;
        }
        
        public void CheckToPickUpGroundItem()
        {
            for (int i = 0; i < groundItems.Count; i++)
            {
                if (groundItems[i].gameObject.activeInHierarchy && groundItems[i].triggerActive)
                {
                    groundItems[i].triggerActive = false;

                    if (groundItems[i].hasAnimator)
                        groundItems[i].objAnimator.SetBool(groundItems[i].animParamName, true);

                    if (groundItems[i].playSfx)
                        SAMSARA.Samsara.Instance.PlaySFXRandomTrack(groundItems[i].itemSfx, out bool success);
                    
                    //Add the item to the player inventory
                    List<Item> items = mapManager.RequestPlayerInventory();

                    if (items.Count >= mapManager.config.inventorySpaces)
                    {
                        //Create prompt and exit: "Your inventory is full."
                        InfoPrompt.Instance.CreatePrompt(new [] {"Your inventory is full."});
                        if (groundItems[i].hasAnimator)
                            groundItems[i].objAnimator.SetBool(groundItems[i].animParamName, false);
                        break;
                    }

                    ItemDatabase database = Resources.Load<ItemDatabase>("Item Database");
                    Item newItem = database.GetItemUsingGuid(groundItems[i].itemID);
                    if (newItem != null)
                    {
                        //Create prompt: $"You found <b><color=yellow>[{newItem.name}]</color></b>."
                        InfoPrompt.Instance.CreatePrompt(new [] {$"You found <b><color=yellow>[{newItem.name}]</color></b>."});
                        groundItems[i].lockTrigger = true;

                        SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_pickup", out bool success);

                        //cringe
                        //  items.Add(newItem);

                        bool foundExisting = false;
                        for (int j = 0; j < items.Count; j++)
                        {
                            if (newItem.guid.Equals(items[j].guid))
                            {
                                if (items[j].stackable)
                                {
                                    foundExisting = true;
                                    items[j].StackSize.x = Mathf.Min(items[j].StackSize.x + newItem.StackSize.x, items[j].StackSize.y);
                                }
                            }
                        }
                        if (!foundExisting)
                        {
                            items.Add(newItem);
                        }

                        mapManager.OverwritePlayerInventory(items);
                        mapManager.currentSlimData.containerStates.Add(groundItems[i].guid);
                    }
                    
                    
                    break;
                }
            }
        }
        
        public GroundItem CheckForActiveGroundItem()
        {
            for (int i = 0; i < groundItems.Count; i++)
            {
                if (groundItems[i] == null) continue;

                if (groundItems[i].triggerActive)
                {
                    return groundItems[i];
                }
            }

            return null;
        }

        private bool CheckValidKeyForItemLock(string lockId, List<Item> inventory, bool destroyKeyOnUse)
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].guid.Equals(lockId))
                {
                    if (destroyKeyOnUse)
                    {
                        mapManager.OverwritePlayerInventory(RemoveKeyItem(lockId, inventory));
                    }

                    return true;
                }
            }

            return false;
        }

        private List<Item> RemoveKeyItem(string idToRemove, List<Item> inventory)
        {
            List<Item> items = new List<Item>(inventory);

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].guid.Equals(idToRemove))
                {
                    items.RemoveAt(i);
                    break;
                }
            }

            return items;
        }

        public void WarpPlayer(Vector3 newPosition, Vector3 newEulers)
        {
            currentPlayer.transform.position = newPosition;
            currentPlayer.transform.rotation = Quaternion.Euler(newEulers);
        }

        public void IncrementEncounterProgress()
        {
            if (randomEncounters)
            {
                encounterProgress += Random.Range(0, encounterProgressRange);
            }
            else
            {
                encounterProgress = 0;
            }
        }

        public void SetRandomEncounters(bool randomEncounter)
        {
            randomEncounters = randomEncounter;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < sceneLoadVariables.Count; i++)
            {
                Gizmos.DrawLine(sceneLoadVariables[i].playerPosition, sceneLoadVariables[i].playerPosition + new Vector3(0, -0.1f, 0) + (Quaternion.Euler(sceneLoadVariables[i].playerRotation) * Vector3.forward) * 0.3f);
            }
        }
    }
}
