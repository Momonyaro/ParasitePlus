using System;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public enum ItemType
    {
        CLUB = 1,       //     Bashing damage (used by Gummo)
        GUN = 2,        //     Piercing damage (used by Sophie)
        KNIFE = 3,      //     Slashing damage (used by Sive)
        AID = 4,        //     General restoratives (bandages, first aid kits, etc...)
        ARMOR = 5,      //     I'm thinking stuff like stab proof vests and body armor.
        KEY = 6,        //     Keys are what they seem
        MISC = 0        //     Everything else.
    }
    
    [CreateAssetMenu(fileName = "Item Database", menuName = "Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<Item> itemDatabase = new List<Item>();
        
        public Item GetItemUsingGuid(string guid)
        {
            Debug.Log("Attempting to retrieve: " + guid);
            for (int i = 0; i < itemDatabase.Count; i++)
            {
                if (itemDatabase[i].guid.Equals(guid))
                {
                    Debug.Log("Found matching item, returning item: " + itemDatabase[i].name);
                    return itemDatabase[i];
                }
            }

            return null;
        }

        public Item[] GetAllItemsOfType(ItemType type, bool onlyStoreItems = false)
        {
            List<Item> bucket = new List<Item>();
            for (int i = 0; i < itemDatabase.Count; i++)
            {
                bool storeCheck = (!onlyStoreItems) || itemDatabase[i].storeItem;
                
                if (itemDatabase[i].type == type && storeCheck)
                {
                    bucket.Add(itemDatabase[i]);
                }
            }

            return bucket.ToArray();
        }
        public Item[] GetAllStoreItems(int maxLevel)
        {
            List<Item> bucket = new List<Item>();
            for (int i = 0; i < itemDatabase.Count; i++)
            {
                bool storeCheck = itemDatabase[i].storeItem && itemDatabase[i].minLevelReq <= maxLevel;

                if (storeCheck)
                {
                    bucket.Add(itemDatabase[i]);
                }
            }

            return bucket.ToArray();
        }
    }

    [System.Serializable]
    public class Item
    {
        public string name; // The name of the item to show in item menus
        public string guid; // id used in comparisons
        public string description;
        public int msrp; // Buying price in stores
        public ItemType type; // Determines where the item can be used and how to sort it.
        
        public int minLevelReq; //The minimum level required to be allowed to buy the item
        public int minFriendshipReq; //The minimum friendship level required to be allowed to buy the item.
        
        public int damage;
        public int defense;
        public float critChance;
        public float dodgeChance;

        public Scriptables.AbilityScriptable itemAbility;

        public bool stackable;
        public Vector2Int StackSize; // if (stackable && maxStackSize == 0) => no stack limit

        public bool storeItem;

        public Item(Item copy)
        {
            this.name = copy.name;
            this.description = copy.description;
            this.type = copy.type;
            this.msrp = copy.msrp;
            this.minLevelReq = copy.minLevelReq;
            this.minFriendshipReq = copy.minFriendshipReq;
            this.guid = copy.guid;
            this.itemAbility = copy.itemAbility;
            this.damage = copy.damage;
            this.defense = copy.defense;
            this.critChance = copy.critChance;
            this.dodgeChance = copy.dodgeChance;
            this.stackable = copy.stackable;
            this.StackSize = copy.StackSize;
            this.storeItem = copy.storeItem;
        }

        public Item(string name, ItemType type, int msrp = 0, int minLevelReq = 0, int minFriendshipReq = 0, int damage = 0, int defense = 0, float critChance = 0, float dodgeChance = 0)
        {
            this.name = name;
            this.type = type;
            this.msrp = msrp;
            this.minLevelReq = minLevelReq;
            this.minFriendshipReq = minFriendshipReq;
            guid = "";
            
            this.damage = damage;
            this.defense = defense;
            this.critChance = critChance;
            this.dodgeChance = dodgeChance;
            this.stackable = false;
            this.StackSize = Vector2Int.zero;
            this.storeItem = false;
        }
    }
}
