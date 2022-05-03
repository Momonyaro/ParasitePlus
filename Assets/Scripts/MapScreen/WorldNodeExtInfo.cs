using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldNodeExtInfo : MonoBehaviour
{
    [SerializeField] private Image mask;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float transitionTime = 0.4f;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subjectText;

    Coroutine transition;

    public void SetVisibility(bool visible)
    {
        if (transition != null)
            StopCoroutine(transition);

        transition = StartCoroutine(IEVisibility(visible).GetEnumerator());
    }

    public void PopulateExtendedInfo(string title, string subject)
    {
        titleText.text = title;
        subjectText.text = subject;
    }

    private IEnumerable IEVisibility(bool visible)
    {
        Vector2 minMax;

        minMax = (visible) ? Vector2.up : Vector2.right;
        float timer = 0;
        float maxTime = transitionTime;

        while (timer < maxTime)
        {
            float lerp = Mathf.Lerp(minMax.x, minMax.y, timer / maxTime);
            
            mask.fillAmount = lerp;
            canvasGroup.alpha = lerp;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        mask.fillAmount = minMax.y;
        canvasGroup.alpha = minMax.y;
    }
}
