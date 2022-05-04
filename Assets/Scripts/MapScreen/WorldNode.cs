using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldNode : MonoBehaviour
{
    //Enumerator for spawning, for hovering and clicking
    [SerializeField] bool alwaysShowExtInfo = false;
    [SerializeField] WorldNodeExtInfo extInfoTab;
    [SerializeField] float interactRange = 50;
    [SerializeField] bool selected = false;

    public string OnPressUIMsg = "";

    private MapController mapController;

    [Header("OnHover Transition")]
    [SerializeField] Vector3 hoverStartScale = Vector3.one;
    [SerializeField] Vector3 hoverTargetScale = Vector3.one;
    [SerializeField] AnimationCurve lerpCurve;

    Coroutine transition;

    public bool IsSelected => selected;

    private void Start()
    {
        mapController = FindObjectOfType<MapController>();

        if (alwaysShowExtInfo)
            extInfoTab.SetVisibility(true);

        SwitchTransition(IEOnStart().GetEnumerator());
    }

    private void Update()
    {
        Vector3 cursorPos = mapController.GetCursorScreenPos;

        float distance = Vector3.Distance(cursorPos, transform.position);


        if (!selected && distance <= interactRange)
        {
            SwitchTransition(IEOnHover().GetEnumerator());
        }
        else if (selected && distance > interactRange)
        {
            SwitchTransition(IEOnRelease().GetEnumerator());
        }
    }

    public void ProcessBtnPress()
    {
        CORE.UIManager.Instance.onUIMessage.Invoke(OnPressUIMsg);
        Debug.Log("Sending message: " + OnPressUIMsg + " to UI Manager!");
    }

    private IEnumerable IEOnStart()
    {
        yield return null;
    }

    private IEnumerable IEOnHover()
    {
        float timer = 0;
        selected = true;
        float maxTimer = lerpCurve.keys[lerpCurve.length - 1].time;

        while (timer < maxTimer)
        {
            float lerp = lerpCurve.Evaluate(timer);
            Vector3 lerped = Vector3.Lerp(hoverStartScale, hoverTargetScale, lerp);

            transform.localScale = lerped;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = hoverTargetScale;

        yield return null;
    }

    private IEnumerable IEOnRelease()
    {
        float timer = 0;
        selected = false;
        float maxTimer = lerpCurve.keys[lerpCurve.length - 1].time;

        while (timer < maxTimer)
        {
            float lerp = lerpCurve.Evaluate(timer);
            Vector3 lerped = Vector3.Lerp(hoverTargetScale, hoverStartScale, lerp);

            transform.localScale = lerped;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = hoverStartScale;

        yield return null;
    }

    private void SwitchTransition(IEnumerator IEtransition)
    {
        if (transition != null)
            StopCoroutine(transition);

        transition = StartCoroutine(IEtransition);
    }
}
