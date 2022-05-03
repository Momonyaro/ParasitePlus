using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldNodePulse : MonoBehaviour
{
    public Image pulseImage;
    public AnimationCurve scaleCurve = AnimationCurve.Linear(0, 1, 1.2f, 3.4f);
    public Gradient colorGradient = new Gradient();
    public bool repeatPulse = false;
    public float delayBetweenPulses = 5.0f;

    private bool firstLoop = true;

    private void Awake()
    {
        StartCoroutine(IEPulse().GetEnumerator());
    }

    private IEnumerable IEPulse()
    {
        while (repeatPulse || firstLoop)
        {
            firstLoop = false;

            float timer = 0;
            float maxTime = scaleCurve.keys[scaleCurve.length - 1].time;

            while (timer < maxTime)
            {
                float curve = scaleCurve.Evaluate(timer);
                Vector3 scale = new Vector3(curve, curve, curve);

                pulseImage.color = colorGradient.Evaluate(timer / maxTime);
                pulseImage.rectTransform.localScale = scale;

                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            float end = scaleCurve.Evaluate(maxTime);

            pulseImage.color = colorGradient.Evaluate(timer / maxTime);
            pulseImage.rectTransform.localScale = new Vector3(end, end, end);

            yield return new WaitForSecondsRealtime(delayBetweenPulses);
        }
    }
}
