using CORE;
using SAMSARA;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public Transform listParent;
    public GameObject listItemPrefab;

    public List<UIButton> listButtons = new List<UIButton>();

    public (string title, string prefsKey)[] optionsData = new (string title, string prefsKey)[]
    {
        ("General Volume", SettingsManager.GENERAL_VOLUME_ID),
        ("Music Volume", SettingsManager.MUSIC_VOLUME_ID),
        ("Vocals Volume", SettingsManager.VOCAL_VOLUME_ID),
        ("Cursor Speed", SettingsManager.CURSOR_SPEED_ID)
    };

    public void PopulateSettings()
    {
        for (int i = 0; i < listParent.childCount; i++)
        {
            Destroy(listParent.GetChild(i).gameObject);
        }
        listButtons.Clear();

        foreach (var option in optionsData)
        {
            GameObject optionGO = Instantiate(listItemPrefab, listParent);

            optionGO.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = option.title;
            TextMeshProUGUI valueText = optionGO.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
            float value = (option.prefsKey == SettingsManager.CURSOR_SPEED_ID) ? PlayerPrefs.GetInt(option.prefsKey) : PlayerPrefs.GetFloat(option.prefsKey);

            if (option.prefsKey == SettingsManager.CURSOR_SPEED_ID)
            {
                valueText.text = SettingsManager.CURSOR_SPEEDS.Where(o => o.offsetValue == (int)value).ToList()[0].optionPrefix;
            }
            else
            {
                valueText.text = (value * 100).ToString("F0") + '%';
            }

            var minusBtn = optionGO.transform.GetChild(1).GetComponent<UIButton>();
            minusBtn.msg = "_sub" + "samsara" + option.prefsKey;
            var plusBtn = optionGO.transform.GetChild(3).GetComponent<UIButton>();
            plusBtn.msg = "_add" + "samsara" + option.prefsKey;

            var buttons = optionGO.transform.GetComponentsInChildren<UIButton>();
            listButtons.AddRange(buttons);
        }
    }

    public void ListenForMessage(string msg)
    {
        if (msg.Equals("_openSettings"))
        {
            PopulateSettings();
        }
        if (msg.StartsWith("_add"))
        {
            bool samsara = msg.Contains("samsara");
            string command = msg.Replace("_add", "").Replace("samsara", "");
            switch(command)
            {
                case SettingsManager.CURSOR_SPEED_ID:
                    var current = GetNextCursorPreset(PlayerPrefs.GetInt(SettingsManager.CURSOR_SPEED_ID));
                    PlayerPrefs.SetInt(SettingsManager.CURSOR_SPEED_ID, current.prefValue);
                    FindObjectOfType<MapController>().ForceUpdateCursorSpeed();
                    break;
                case SettingsManager.GENERAL_VOLUME_ID:
                case SettingsManager.MUSIC_VOLUME_ID:
                case SettingsManager.VOCAL_VOLUME_ID:
                    if (samsara)
                    {
                        var getFloat = AddVolume(PlayerPrefs.GetFloat(command));
                        PlayerPrefs.SetFloat(command, getFloat);
                        Samsara.Instance.UpdateVolumeGroups();
                    }
                    break;
            }
            PopulateSettings();
        }
        if (msg.StartsWith("_sub"))
        {
            bool samsara = msg.Contains("samsara");
            string command = msg.Replace("_sub", "").Replace("samsara", "");
            switch (command)
            {
                case SettingsManager.CURSOR_SPEED_ID:
                    var current = GetPrevCursorPreset(PlayerPrefs.GetInt(SettingsManager.CURSOR_SPEED_ID));
                    PlayerPrefs.SetInt(SettingsManager.CURSOR_SPEED_ID, current.prefValue);
                    FindObjectOfType<MapController>().ForceUpdateCursorSpeed();
                    break;
                case SettingsManager.GENERAL_VOLUME_ID:
                case SettingsManager.MUSIC_VOLUME_ID:
                case SettingsManager.VOCAL_VOLUME_ID:
                    if (samsara)
                    {
                        var getFloat = SubVolume(PlayerPrefs.GetFloat(command));
                        PlayerPrefs.SetFloat(command, getFloat);
                        Samsara.Instance.UpdateVolumeGroups();
                    }
                    break;
            }
            PopulateSettings();
        }
    }

    private (string title, int prefValue) GetNextCursorPreset(int value)
    {
        int currentIndex = 0;
        var presets = SettingsManager.CURSOR_SPEEDS;
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i].offsetValue == value)
                currentIndex = i;
        }

        currentIndex++;

        if (currentIndex == presets.Length) //loop around
            currentIndex = 0;

        return presets[currentIndex];
    }

    private (string title, int prefValue) GetPrevCursorPreset(int value)
    {
        int currentIndex = 0;
        var presets = SettingsManager.CURSOR_SPEEDS;
        for (int i = 0; i < presets.Length; i++)
        {
            if (presets[i].offsetValue == value)
                currentIndex = i;
        }

        currentIndex--;

        if (currentIndex == -1) //loop around
            currentIndex = presets.Length - 1;

        return presets[currentIndex];
    }

    private float AddVolume(float volume)
    {
        return Mathf.Clamp01(volume + 0.05f);
    }

    private float SubVolume(float volume)
    {
        return Mathf.Clamp01(volume - 0.05f);
    }

    private void OnEnable()
    {
        UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
    }

    private void OnDisable()
    {
        CORE.UIManager.Instance.onUIMessage.RemoveListener(ListenForMessage);
    }
}
