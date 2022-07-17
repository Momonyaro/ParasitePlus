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
    [SerializeField] bool hidden = false;

    public string OnPressUIMsg = "";
    public int MinButtonLayer = 0;
    public int MaxButtonLayer = 0;

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

        if (!IsInRange(WorldManager.CurrentSelectionLayer, MinButtonLayer, MaxButtonLayer))
        {
            Debug.Log(WorldManager.CurrentSelectionLayer);
            gameObject.SetActive(false);
        }
        else
            SwitchTransition(IEOnStart().GetEnumerator());
    }

    private void Update()
    {
        Vector3 cursorPos = mapController.GetCursorScreenPos;

        float distance = Vector3.Distance(cursorPos, transform.position);

        if (!hidden && !IsInRange(WorldManager.CurrentSelectionLayer, MinButtonLayer, MaxButtonLayer))
        {
            SwitchTransition(IEOnHide().GetEnumerator());
            return;
        }
        else if (hidden && IsInRange(WorldManager.CurrentSelectionLayer, MinButtonLayer, MaxButtonLayer))
        {
            SwitchTransition(IEOnStart().GetEnumerator());
            return;
        }


        if (!selected && distance <= interactRange)
        {
            SwitchTransition(IEOnHover().GetEnumerator());
        }
        else if (selected && distance > interactRange)
        {
            SwitchTransition(IEOnRelease().GetEnumerator());
        }
    }

    /// <summary>
    /// Check if number is in range of min max.
    /// </summary>
    /// <param name="current"></param>
    /// <param name="min">Min is the range minimum. Inclusive.</param>
    /// <param name="max">Max is the range maximum. Inclusive.</param>
    /// <returns></returns>
    public bool IsInRange(int current, int min, int max)
    {
        if (current < min || current > max) return false;
        return true;
    }

    public void ProcessBtnPress()
    {
        CORE.UIManager.Instance.onUIMessage.Invoke(OnPressUIMsg);
        Debug.Log("Sending message: " + OnPressUIMsg + " to UI Manager!");
    }

    private IEnumerable IEOnStart()
    {
        if (alwaysShowExtInfo)
            extInfoTab.SetVisibility(true);

        hidden = false;

        yield return null;
    }

    private IEnumerable IEOnHover()
    {
        float timer = 0;
        selected = true;
        float maxTimer = lerpCurve.keys[lerpCurve.length - 1].time;

        if (!alwaysShowExtInfo)
            extInfoTab.SetVisibility(true);

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

        if (!alwaysShowExtInfo)
            extInfoTab.SetVisibility(false);

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

    private IEnumerable IEOnHide()
    {
        if (alwaysShowExtInfo)
            extInfoTab.SetVisibility(false);

        hidden = true;

        yield return null;
    }

    private void SwitchTransition(IEnumerator IEtransition)
    {
        if (transition != null)
            StopCoroutine(transition);

        transition = StartCoroutine(IEtransition);
    }
}
