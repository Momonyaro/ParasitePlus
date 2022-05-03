using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrolRawImg : MonoBehaviour
{
    public float timeForFullScroll = 0.5f;
    public RawImage rawImage;

    private void Awake()
    {
        StartCoroutine(IEScroll().GetEnumerator());
    }

    private IEnumerable IEScroll()
    {
        Vector2 minMax = new Vector2(0, 1);
        float timer = 0;

        while (true)
        {
            timer = 0;
            float maxTime = timeForFullScroll;

            while (timer < maxTime)
            {
                Rect uvRect = rawImage.uvRect;

                uvRect.x = Mathf.Lerp(minMax.x, minMax.y, timer / maxTime);

                rawImage.uvRect = uvRect;

                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
    }

}
