using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoreCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform lerpStart;
    public Transform lerpEnd;
    public AnimationCurve lerpCurve;

    public UnityEvent onEntrance = new UnityEvent();


    private void Start()
    {
        StartCoroutine(IEStoreEntrance().GetEnumerator());
    }

    private void OnDestroy()
    {
        onEntrance.RemoveAllListeners();
    }

    private IEnumerable IEStoreEntrance()
    {
        cameraTransform.position = lerpStart.position;

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        while(timer < maxTimer)
        {
            cameraTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, lerpCurve.Evaluate(timer));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        cameraTransform.position = lerpEnd.position;

        onEntrance.Invoke();

        yield break;
    }
}
