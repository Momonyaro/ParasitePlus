using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkylineTimelapse : MonoBehaviour
{
    public Sprite sunSprite;
    public Sprite moonSprite;

    public Image skyImage;
    public Image sunImage;
    public Image moonImage;
    public Image groundImage;
    public Image waterImage;
    public RectTransform sunMoonTransform;

    [SerializeField] AnimationCurve sunMoonCurve;
    [SerializeField] AnimationCurve progressCurve;
    [SerializeField] Gradient skyGradient;
    [SerializeField] Gradient groundGradient;
    [SerializeField] Gradient waterGradient;

    private UI.FadeToBlackImage fadeToBlackImage;

    private void Start()
    {
        StartCoroutine(IESkyline().GetEnumerator());
        fadeToBlackImage = FindObjectOfType<UI.FadeToBlackImage>();
    }


    private IEnumerable IESkyline()
    {
        float timer = 0;
        float maxTimer = progressCurve.keys[progressCurve.length - 1].time;

        while (timer < maxTimer)
        {
            float lerp = progressCurve.Evaluate(timer);
            skyImage.color = skyGradient.Evaluate(lerp);
            groundImage.color = groundGradient.Evaluate(lerp);
            waterImage.color = waterGradient.Evaluate(lerp);
            sunMoonTransform.rotation = Quaternion.Euler(0, 0, sunMoonCurve.Evaluate(timer));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Color original = fadeToBlackImage.color;
        fadeToBlackImage.color = Color.black;
        yield return fadeToBlackImage.FadeToBlackEnumerator(0.3f, 0.5f, true);
        fadeToBlackImage.color = original;

        GotoDestination();
    }

    private void GotoDestination()
    {
        CORE.UIManager.Instance.onUIMessage.Invoke("_loadNextScene");
    }
}
