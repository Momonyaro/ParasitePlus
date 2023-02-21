using BattleSystem;
using BattleSystem.UI;
using Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "SummonInterject", menuName = "Interjects/Summon Interject", order = 1)]
public class SummonInterject : InterjectBase
{
    public DialogueNode successText;
    public DialogueNode failureText;
    public EntityScriptable enemy;
    [Range(0, 1)] public float summonChance = 0.5f;

    DialogueScreenUI dialogueScreen;
    BattleSystemEnemyField enemyField;

    public override void Init()
    {
        dialogueScreen = FindObjectOfType<DialogueScreenUI>();

        enemyField = FindObjectOfType<BattleSystemEnemyField>();
        List<EntityScriptable> entities = new List<EntityScriptable>() { null, null, enemy, null, null };

        for (int i = 0; i < entities.Count; i++)
        {
            if (entities[i] == null) continue;
            entities[i] = entities[i].Copy();
        }

        entities = Shuffle(entities, new System.Random(Mathf.FloorToInt(Time.time)));

        //Check success / failure
        bool success = false;
        if (summonChance > Random.value)
        {
            success = true;
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i] == null) continue;
                battleCore.AddEntityToBattle(i, entities[i]);
            }

            enemyField.PopulateField(battleCore.enemyField);
        }

        dialogueScreen.StartDialogue(new List<DialogueNode>() { success ? successText : failureText });

        initialized = true;
    }

    public override void UpdateState(out bool endOfLife)
    {
        endOfLife = !dialogueScreen.IsRunning();
    }

    public override void Disconnect()
    {
        initialized = false;
    }

    public override void OnSubmitButton(InputAction.CallbackContext obj)
    {
        dialogueScreen.UpdateDialogue();
    }

    private static List<T> Shuffle<T>(List<T> list, System.Random rnd)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var randomIndex = rnd.Next(i + 1); //maxValue (i + 1) is EXCLUSIVE
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        return list;
    }
}
