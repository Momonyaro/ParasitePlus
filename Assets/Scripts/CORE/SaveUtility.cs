using Items;
using Scriptables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

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

        if (SaveExists())
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

        if (SaveExists())
        {
            string json = System.IO.File.ReadAllText(SavePath + $"/{SaveFileName}");
            SaveItem loaded = JsonUtility.FromJson<SaveItem>(json);
            success = true;
            return ConvertToSlimData(loaded);
        }

        return null;
    }

    public static bool SaveExists()
    {
        return System.IO.File.Exists(SavePath + $"/{SaveFileName}");
    }

    public static void WipeSave()
    {
        if (SaveExists())
        {
            string temp = System.IO.File.ReadAllText(SavePath + $"/{SaveFileName}");
            System.IO.File.WriteAllText(SavePath + $"/{OldSaveFileName}", temp);
            System.IO.File.Delete(SavePath + $"/{SaveFileName}");
        }
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
            interactableStates = ArrayToDictionary(saveItem.InteractableStates)
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

    [System.Serializable]
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
        public FakeTuple[] InteractableStates;

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
            InteractableStates = DictionaryToArray(slimData.interactableStates);
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

    private static FakeTuple[] DictionaryToArray(Dictionary<string, bool> dictionary)
    {
        if (dictionary == null)
            return new FakeTuple[0];

        List<FakeTuple> list = new List<FakeTuple>();

        foreach (KeyValuePair<string, bool> kvp in dictionary)
        {
            list.Add(new FakeTuple(kvp.Key, kvp.Value));
        }

        return list.ToArray();
    }

    private static Dictionary<string, bool> ArrayToDictionary(FakeTuple[] array)
    {
        if (array == null)
            return new Dictionary<string, bool>();

        List<FakeTuple> list = new List<FakeTuple>(array);

        var dictionary = list.ToDictionary(x => x.key, x => x.value);

        return dictionary;
    }

    [System.Serializable]
    public struct FakeTuple
    {
        public string key;
        public bool value;

        public FakeTuple(string key, bool value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
