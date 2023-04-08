using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarTracker : MonoBehaviour
{
    public GameObject starPrefab;
    public Transform parent;

    private void Start()
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Destroy(parent.GetChild(i).gameObject);
        }

        const string prefsKey = SettingsManager.GAME_COMPLETED_ID;

        if (PlayerPrefs.HasKey(prefsKey))
        {
            int completedAmount = PlayerPrefs.GetInt(prefsKey);

            for (int i = 0; i < completedAmount; i++)
            {
                Instantiate(starPrefab, parent);
            }
        }
    }
}
