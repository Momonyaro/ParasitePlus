using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompletionTrigger : MonoBehaviour
{
    private void Start()
    {
        TriggerEvent();
    }

    public void TriggerEvent()
    {
        SaveUtility.WipeSave();
        const string prefsKey = SettingsManager.GAME_COMPLETED_ID;

        if (PlayerPrefs.HasKey(prefsKey))
        {
            var current = PlayerPrefs.GetInt(prefsKey);
            PlayerPrefs.SetInt(prefsKey, current + 1);
        }
        else
        {
            PlayerPrefs.SetInt(prefsKey, 1);
        }
    }
}
