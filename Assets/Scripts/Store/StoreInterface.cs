using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreInterface : MonoBehaviour
{
    public Image background;
    public AnimationCurve lerpCurve;
    public ItemBtn[] itemButtons = new ItemBtn[0];
    public ItemBtn nextPageButton;
    public ItemBtn prevPageButton;

    private void Awake()
    {
        background.fillAmount = 0;
        foreach (ItemBtn itemBtn in itemButtons)
        {
            itemBtn.active = false;
        }
        prevPageButton.active = false;
        nextPageButton.active = false;
    }

    private IEnumerator IEOpen()
    {
        StoreManager manager = FindObjectOfType<StoreManager>();

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        while (timer < maxTimer)
        {
            background.fillAmount = lerpCurve.Evaluate(timer);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        background.fillAmount = lerpCurve.Evaluate(maxTimer);

        yield break;
    }
}
