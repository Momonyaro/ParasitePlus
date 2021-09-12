﻿using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using MapTriggers;
using MOVEMENT;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public bool enterOnUnlock = true;
        public List<EncounterTrigger> encounterTriggers = new List<EncounterTrigger>();
        public List<EventTrigger> eventTriggers = new List<EventTrigger>();

        private FPSGridPlayer currentPlayer;
        private MapManager mapManager;

        private void Update()
        {
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
                    mapManager.currentSlimData.usedGroundItems.Add(eventTriggers[i].guid);
                }
            }
        }

        public void TransitionToBattleFromTrigger(EncounterTrigger trigger)
        {
            StartCoroutine(TransitionToBattle(trigger));
        }

        private IEnumerator TransitionToBattle(EncounterTrigger trigger)
        {
            mapManager.SetEnemyField(trigger.enemyRoster);
            trigger.triggerActive = false;
            mapManager.currentSlimData.usedGroundItems.Add(trigger.guid);
            
            FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();
            
            fadeToBlackImage.FadeToBlack(0.3f, .5f, true);
            
            while (!fadeToBlackImage.finished) { yield return null; }
            
            mapManager.SwitchSceneToBattle(trigger.postBattleSceneName);
            
            yield break;
        }

        public void SetPlayer(MapManager manager, FPSGridPlayer player)
        {
            this.currentPlayer = player;
            mapManager = manager;
        }

        public void CheckToEnterDoorTrigger()
        {
            for (int i = 0; i < doorInteractables.Count; i++)
            {
                if (doorInteractables[i] == null) continue;
                
                //Add new doors for the FPS sections
                if (doorInteractables[i].triggerActive)
                {
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
                    StartCoroutine(WaitForDoorTranstion(i));
                    break;
                }
            }
        }
        
        private IEnumerator WaitForDoorTranstion(int currentIndex)
        {
            FadeToBlackImage fadeToBlackImage = FindObjectOfType<FadeToBlackImage>();
            currentPlayer.lockPlayer = true;
            
            fadeToBlackImage.FadeToBlack(0.3f, .8f);
            
            while (!fadeToBlackImage.screenBlack) { yield return null; }
            
            Vector3 camEulers = currentPlayer.transform.rotation.eulerAngles;
            WarpPlayer(doorInteractables[currentIndex].warpDest, camEulers);
            
            while (!fadeToBlackImage.finished) { yield return null; }
            
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
                    groundItems[i].gameObject.SetActive(false);
                    
                    //Add the item to the player inventory
                    List<Item> items = mapManager.RequestPlayerInventory();

                    if (items.Count >= mapManager.config.inventorySpaces)
                    {
                        //Create prompt and exit: "Your inventory is full."
                        InfoPrompt.Instance.CreatePrompt(new [] {"Your inventory is full."});
                        break;
                    }

                    ItemDatabase database = Resources.Load<ItemDatabase>("Item Database");
                    Item newItem = database.GetItemUsingGuid(groundItems[i].itemID);
                    if (newItem != null)
                    {
                        //Create prompt: $"You found <b><color=yellow>[{newItem.name}]</color></b>."
                        InfoPrompt.Instance.CreatePrompt(new [] {$"You found <b><color=yellow>[{newItem.name}]</color></b>."});
                        
                        items.Add(newItem);
                        mapManager.OverwritePlayerInventory(items);
                        mapManager.currentSlimData.usedGroundItems.Add(groundItems[i].guid);
                    }
                    
                    
                    break;
                }
            }
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
        
    }
}
