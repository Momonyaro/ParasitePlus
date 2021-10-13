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
    public Queue<string> currentMessage = new Queue<string>();
    private bool combatOnClear = false;
    private EncounterTrigger cachedTrigger = null;

    private static string _infoPromptAnimatorVarName = "ShowPrompt";
    
    private void Awake()
    {
        Instance = this;
    }

    public void CreatePrompt(string[] promptMessage)
    {
        if (promptMessage.Length == 0)
        {
            return;
        }
        
        FindObjectOfType<FPSGridPlayer>().lockPlayer = true;
        currentMessage = new Queue<string>(promptMessage);
        
        promptText.text = currentMessage.Dequeue();
        promptAnimator.SetBool(_infoPromptAnimatorVarName, true);
        combatOnClear = false;
    }

    public void CreatePromptForCombat(string[] promptMessage, EncounterTrigger trigger)
    {
        FindObjectOfType<FPSGridPlayer>().lockPlayer = true;
        currentMessage = new Queue<string>(promptMessage);
        
        promptText.text = currentMessage.Dequeue();
        promptAnimator.SetBool(_infoPromptAnimatorVarName, true);
        combatOnClear = true;
        cachedTrigger = trigger;
    }

    public void ClearPrompt()
    {
        if (currentMessage.Count > 0)
        {
            promptText.text = currentMessage.Dequeue();
            promptAnimator.SetTrigger("RepeatReveal");
            return;
        }
        
        FindObjectOfType<FPSGridPlayer>().lockPlayer = false;

        promptAnimator.SetBool(_infoPromptAnimatorVarName, false);

        if (combatOnClear)
        {
            combatOnClear = false;
            FindObjectOfType<DungeonManager>().TransitionToBattleFromTrigger(cachedTrigger);
        }
    }
}
