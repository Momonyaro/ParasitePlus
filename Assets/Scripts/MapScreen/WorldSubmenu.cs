using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WorldSubmenu : MonoBehaviour
{
    public Vector2 MinMaxVector = new Vector2(0, 4000);
    public AnimationCurve lerpCurve = new AnimationCurve();
    public Image mask;
    public bool visible = false;
    public UnityEvent onOpenSubmenu;

    private Coroutine transition;

    private void Awake()
    {
        onOpenSubmenu = new UnityEvent();
    }

    public void SetVisibility(bool visibility)
    {
        visible = visibility;

        onOpenSubmenu.Invoke();
        SwitchTransition(IEVisibility(visible).GetEnumerator());
    }

    private IEnumerable IEVisibility(bool visible)
    {
        Vector2 minMax;

        minMax = (visible) ? MinMaxVector : new Vector2(MinMaxVector.y, MinMaxVector.x);
        float timer = 0;
        float maxTime = lerpCurve.keys[lerpCurve.length - 1].time;

        while (timer < maxTime)
        {
            float lerp = Mathf.Lerp(minMax.x, minMax.y, lerpCurve.Evaluate(timer));

            Vector2 temp = mask.rectTransform.sizeDelta;
            temp.x = temp.y = lerp;
            mask.rectTransform.sizeDelta = temp;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Vector2 temp2 = mask.rectTransform.sizeDelta;
        temp2.x = temp2.y = minMax.y;
        mask.rectTransform.sizeDelta = temp2;
    }

    private void SwitchTransition(IEnumerator IEtransition)
    {
        if (transition != null)
            StopCoroutine(transition);

        transition = StartCoroutine(IEtransition);
    }
}
