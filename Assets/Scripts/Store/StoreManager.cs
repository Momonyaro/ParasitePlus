using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    //Read the slim if it exists.

    //Read the item database
    //Filter items based on player level.

    //User can use 3 main filters to sort shop [] alphabetical, price, min lvl. (recent filter)
    //User playerutils to store last sorting mode and last level the player was when they entered. (default value: 0)

    public CORE.SlimComponent.SlimData currentSlim = new CORE.SlimComponent.SlimData();

    public const string LastPlayerLevelPrefsKey = "Store::PlayerLastLevel";

    private int lastPlayerLevel = 0;

    private void Start()
    {
        CORE.SlimComponent.Instance.ReadVolatileSlim(out CORE.SlimComponent.SlimData foundData);
        LoadSlim(foundData);

        lastPlayerLevel = GetLastPlayerLevel(out bool success);
        WritePlayerLevel(currentSlim.partyField[0].entityLevel);

        Debug.Log($"Fetched level: {lastPlayerLevel}, wrote level: {currentSlim.partyField[0].entityLevel}");
    }

    private void LoadSlim(CORE.SlimComponent.SlimData foundData)
    {
        if (foundData.partyField.Length < 4) return;

        currentSlim = foundData;

        for (int i = 0; i < currentSlim.partyField.Length; i++)
        {
            if (currentSlim.partyField[i] != null)
                currentSlim.partyField[i] = currentSlim.partyField[i].Copy();
        }
    }
    
    private void WritePlayerLevel(int level)
    {
        PlayerPrefs.SetInt(LastPlayerLevelPrefsKey, level);
    }

    //This is used to show the "NEW" tag on items the player couldn't get before.
    private int GetLastPlayerLevel(out bool success)
    {
        success = PlayerPrefs.HasKey(LastPlayerLevelPrefsKey);
        return PlayerPrefs.GetInt(LastPlayerLevelPrefsKey, 0);
    }
}

[CustomEditor(typeof(StoreManager))]
public class StoreManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Reset Last Player Level"))
            PlayerPrefs.DeleteKey(StoreManager.LastPlayerLevelPrefsKey);

        base.OnInspectorGUI();
    }
}