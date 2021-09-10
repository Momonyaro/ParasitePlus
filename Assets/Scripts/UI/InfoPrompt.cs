using System;
using System.Collections;
using System.Collections.Generic;
using CORE;
using MapTriggers;
using MOVEMENT;
using TMPro;
using UnityEngine;

public class InfoPrompt : MonoBehaviour
{
    public static InfoPrompt Instance;

    public Animator promptAnimator;
    public TextMeshProUGUI promptText;
    private bool combatOnClear = false;
    private EncounterTrigger cachedTrigger = null;

    private static string _infoPromptAnimatorVarName = "ShowPrompt";
    
    private void Awake()
    {
        Instance = this;
    }

    public void CreatePrompt(string promptMessage)
    {
        FindObjectOfType<FPSGridPlayer>().lockPlayer = true;
        
        promptText.text = promptMessage;
        promptAnimator.SetBool(_infoPromptAnimatorVarName, true);
        combatOnClear = false;
    }

    public void CreatePromptForCombat(string promptMessage, EncounterTrigger trigger)
    {
        FindObjectOfType<FPSGridPlayer>().lockPlayer = true;
        
        promptText.text = promptMessage;
        promptAnimator.SetBool(_infoPromptAnimatorVarName, true);
        combatOnClear = true;
        cachedTrigger = trigger;
    }

    public void ClearPrompt()
    {
        FindObjectOfType<FPSGridPlayer>().lockPlayer = false;
        
        promptAnimator.SetBool(_infoPromptAnimatorVarName, false);

        if (combatOnClear)
        {
            combatOnClear = false;
            FindObjectOfType<DungeonManager>().TransitionToBattleFromTrigger(cachedTrigger);
        }
    }
}
