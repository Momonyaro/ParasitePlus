using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AttackPromptUI : MonoBehaviour
{
    [SerializeField] private Transform uiTransform;
    [SerializeField] private TextMeshProUGUI text;

    public void ShowAttackPrompt(string promptMessage, float duration)
    {
        text.text = promptMessage;
        uiTransform.gameObject.SetActive(true);

        StartCoroutine(IEShowAttackPrompt(duration));
    }

    private IEnumerator IEShowAttackPrompt(float duration)
    {
        yield return new WaitForSeconds(duration);
        uiTransform.gameObject.SetActive(false);
    }
}
