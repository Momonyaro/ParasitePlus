using Items;
using Scriptables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveUtility : MonoBehaviour
{
    public const string SaveFileName = "save.bps";
    public const string OldSaveFileName = "old__save.bps";
    public static string SavePath = Application.persistentDataPath;
    
    public bool WritingToDisk { get; private set; } = false;

    public static void WriteToDisk(CORE.SlimComponent.SlimData saveData)
    {
        SaveItem save = new SaveItem(saveData);
        
        string json = JsonUtility.ToJson(save, true);

        if (System.IO.File.Exists(SavePath + $"/{SaveFileName}"))
        {
            string temp = System.IO.File.ReadAllText(SavePath + $"/{SaveFileName}");
            System.IO.File.WriteAllText(SavePath + $"/{OldSaveFileName}", temp);
        }

        System.IO.File.WriteAllText(SavePath + $"/{SaveFileName}", json);
        Debug.Log(json);
    }

    public static CORE.SlimComponent.SlimData ReadFromDisk(out bool success)
    {
        success = false;

        if (System.IO.File.Exists(SavePath + $"/{SaveFileName}"))
        {
            string json = System.IO.File.ReadAllText(SavePath + $"/{SaveFileName}");
            SaveItem loaded = JsonUtility.FromJson<SaveItem>(json);
            success = true;
            return ConvertToSlimData(loaded);
        }

        return null;
    }

    private static CORE.SlimComponent.SlimData ConvertToSlimData(SaveItem saveItem)
    {
        CORE.SlimComponent.SlimData newData = new CORE.SlimComponent.SlimData()
        {
            playerName = saveItem.PlayerName,
            inventory = new List<Item>(saveItem.Inventory),
            partyField = GetPartyFromSave(saveItem.PartyField),
            wallet = saveItem.Wallet,

            lastTransformScene = saveItem.LastTransformScene,
            ignoreTransformReadFlag = saveItem.IgnoreTransformFlag,
            playerLastPos = saveItem.PlayerPos,
            playerLastEuler = saveItem.PlayerRot,

            destinationScene = saveItem.DestinationScene,
            loadSceneVariable = saveItem.LoadSceneVariable,
            lastButtonLayer = saveItem.ButtonLayer,

            eventTriggers = ArrayToHashset<string>(saveItem.EventTriggers),
            containerStates = ArrayToHashset<string>(saveItem.ContainerStates),
            interactableStates = ArrayToDictionary<string, bool>(saveItem.InteractableStates)
        };

        return newData;
    }

    private static EntityScriptable[] GetPartyFromSave(SaveItem.PartyData[] party)
    {
        EntityScriptable[] toReturn = new EntityScriptable[4];
        EntityScriptable[] inResources = Resources.LoadAll<EntityScriptable>("");

        for (int i = 0; i < party.Length; i++)
        {
            EntityScriptable current = null;
            for (int j = 0; j < inResources.Length; j++)
            {
                if (party[i]._ID.Equals(inResources[j].entityId))
                    current = inResources[j];
            }

            if (current != null)
            {
                current.entityName = party[i].Name;
                current.entityLevel = party[i].Level;
                current.entityXp = party[i].XP.x;
                current.entityXpThreshold = party[i].XP.y;
                current.inParty = party[i].InParty;
                current.SetEntityHP(party[i].HP);
                current.SetEntityAP(party[i].AP);

                toReturn[i] = current;
            }
        }

        return toReturn;
    }

    public struct SaveItem
    {
        public string PlayerName;
        public PartyData[] PartyField;
        public Item[] Inventory;
        public int Wallet;

        public string LastTransformScene;
        public bool IgnoreTransformFlag;
        public Vector3 PlayerPos;
        public Vector3 PlayerRot;

        public string DestinationScene;
        public string LoadSceneVariable;
        public int ButtonLayer;

        public string[] EventTriggers;
        public string[] ContainerStates;
        public Tuple<string, bool>[] InteractableStates;

        public SaveItem(CORE.SlimComponent.SlimData slimData)
        {
            PlayerName = slimData.playerName;

            PartyField = new PartyData[slimData.partyField.Length];
            for (int i = 0; i < slimData.partyField.Length; i++)
            {
                if (slimData.partyField[i] == null || slimData.partyField[i].entityId.Equals(""))
                {
                    PartyField[i] = null;
                    continue;
                }

                PartyField[i] = new PartyData(slimData.partyField[i]);
            }

            Inventory = slimData.inventory.ToArray();
            Wallet = slimData.wallet;

            LastTransformScene = slimData.lastTransformScene;
            IgnoreTransformFlag = slimData.ignoreTransformReadFlag;
            PlayerPos = slimData.playerLastPos;
            PlayerRot = slimData.playerLastEuler;
            PlayerRot.y = Mathf.Round(PlayerRot.y);

            DestinationScene = slimData.destinationScene;
            LoadSceneVariable = slimData.loadSceneVariable;
            ButtonLayer = slimData.lastButtonLayer;

            //Funky event recontextualizing
            EventTriggers = HashsetToArray<string>(slimData.eventTriggers);
            ContainerStates = HashsetToArray<string>(slimData.containerStates);
            InteractableStates = DictionaryToArray<string, bool>(slimData.interactableStates);
        }
    
        [System.Serializable]
        public class PartyData
        {
            public string Name;
            public string _ID;
            public Vector2Int HP;
            public Vector2Int AP;

            public int Level;
            public Vector2Int XP;

            public bool InParty;

            public PartyData(EntityScriptable entity)
            {
                Name = entity.entityName;
                _ID = entity.entityId;
                HP = entity.GetEntityHP();
                AP = entity.GetEntityAP();

                Level = entity.entityLevel;
                XP = new Vector2Int(entity.entityXp, entity.entityXpThreshold);
                InParty = entity.inParty;
            }
        }
    }



    private static T[] HashsetToArray<T>(HashSet<T> hashset)
    {
        if (hashset == null)
            return new T[0];

        T[] array = new T[hashset.Count];
        hashset.CopyTo(array);

        return array;
    }

    private static HashSet<T> ArrayToHashset<T>(T[] array)
    {
        if (array == null)
            return new HashSet<T>();

        HashSet<T> hashset = new HashSet<T>(array);
        return hashset;
    }

    private static Tuple<TKey, TValue>[] DictionaryToArray<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null)
            return new Tuple<TKey, TValue>[0];

        List<Tuple<TKey, TValue>> list = new List<Tuple<TKey, TValue>>();

        foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
        {
            list.Add(new Tuple<TKey, TValue>(kvp.Key, kvp.Value));
        }

        return list.ToArray();
    }

    private static Dictionary<TKey, TValue> ArrayToDictionary<TKey, TValue>(Tuple<TKey, TValue>[] array)
    {
        if (array == null)
            return new Dictionary<TKey, TValue>();

        List<Tuple<TKey, TValue>> list = new List<Tuple<TKey, TValue>>(array);

        var dictionary = list.ToDictionary(x => x.Item1, x => x.Item2);

        return dictionary;
    }
}
