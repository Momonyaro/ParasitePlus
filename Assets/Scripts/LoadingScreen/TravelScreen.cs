using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TravelScreen : MonoBehaviour
{

    public Image skyImage;
    public RawImage backgroundImage;
    public Image foregroundImage;
    public RectTransform[] wavyObjects;
    public float wavyStrength = 2;

    public float sceneTimer = 4f;

    [SerializeField] AnimationCurve shakeCurve;
    public float shakeStrength = 2;
    public float scrollSpeed = 0.3f;

    private UI.FadeToBlackImage fadeToBlackImage;

    private void Start()
    {
        StartCoroutine(IESkyline().GetEnumerator());
        fadeToBlackImage = FindObjectOfType<UI.FadeToBlackImage>();
    }

    private void Update()
    {
        for (int i = 0; i < wavyObjects.Length; i++)
        {
            wavyObjects[i].rotation = Quaternion.Euler(0, 0, Mathf.PerlinNoise(wavyObjects[i].position.x, Time.time) * wavyStrength);
        }

        foregroundImage.rectTransform.anchoredPosition = new Vector2(0, shakeCurve.Evaluate(Time.time) * shakeStrength);

        backgroundImage.uvRect = new Rect(Time.timeSinceLevelLoad * scrollSpeed, 0, 1, 1);
    }

    private IEnumerable IESkyline()
    {
        float timer = 0;
        float maxTimer = sceneTimer;

        while (timer < maxTimer)
        {
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Color original = fadeToBlackImage.color;
        fadeToBlackImage.color = Color.black;
        yield return fadeToBlackImage.FadeToBlackEnumerator(0.3f, 0.5f, true);
        fadeToBlackImage.color = original;

        GotoDestination();

        yield break;
    }


    private void GotoDestination()
    {
        CORE.UIManager.Instance.onUIMessage.Invoke("_loadNextScene");
    }
}
