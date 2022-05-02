using MapTriggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyMasterEvent : MonoBehaviour
{
    public DoorInteractable[] doors = new DoorInteractable[0];
    private CORE.MapManager mapManager;

    public void SetDoorLockState(bool newState)
    {
        mapManager = FindObjectOfType<CORE.MapManager>();

        //go through each door, update it's lock state and save the door to persistant data.
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].locked = !newState;
            OverwriteDictionaryEntry(doors[i].guid, doors[i].locked);
        }
    }

    private void OverwriteDictionaryEntry(string key, bool value)
    {
        if (mapManager.currentSlimData.interactableStates.ContainsKey(key))
            mapManager.currentSlimData.interactableStates[key] = value;
        else
            mapManager.currentSlimData.interactableStates.Add(key, value);
    }
}
