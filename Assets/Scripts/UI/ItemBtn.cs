using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemBtn : MonoBehaviour
{
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI secondText;
    public string itemDescription;
    public Image background;
    public AnimationCurve introFillCurve;

    public Color defaultCol = Color.white;
    public Color selectedCol = Color.white;
    public Color defaultTextCol = Color.black;
    public Color selectedTextCol = Color.black;

    public bool hovering = false;
    public bool active = false;

    public UnityEvent<string> onPress;

    private string storedMsg;
    private MapController mController;

    private void Awake()
    {
        mController = FindObjectOfType<MapController>();
    }

    private void Update()
    {
        if (!active) return;

        bool inside = IsInsideRect(mController.GetCursorScreenPos);

        if (!hovering && inside)
        {
            OnCursorEnter();
        }
        else if (hovering && !inside)
            OnCursorExit();
    }

    //This won't work with gamepads, re-do
    public void OnCursorClick()
    {
        if (!active) return;
        onPress.Invoke(storedMsg);
    }

    public void OnCursorEnter()
    {
        mainText.color = selectedTextCol;
        secondText.color = selectedTextCol;
        background.color = selectedCol;
        hovering = true;

        ItemDetailWindow.Instance.CreateDetailWindow(mainText.text, itemDescription);
    }

    public void OnCursorExit()
    {
        mainText.color = defaultTextCol;
        secondText.color = defaultTextCol;
        background.color = defaultCol;
        hovering = false;
    }

    public void SetButtonData(string title, string extra, string toStore, string itemDescription)
    {
        mainText.text = title;
        secondText.text = extra;
        this.itemDescription = itemDescription;
        StoreMessage(toStore);

        StopCoroutine(IEIntroFill());
        StartCoroutine(IEIntroFill());

        IEnumerator IEIntroFill()
        {
            float timer = 0;
            float maxTime = introFillCurve.keys[introFillCurve.length - 1].time;

            while (timer < maxTime)
            {
                background.fillAmount = introFillCurve.Evaluate(timer);

                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            background.fillAmount = introFillCurve.Evaluate(maxTime);

            yield break;
        }
    }

    public bool IsInsideRect(Vector2 pos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(background.rectTransform, pos);
    }

    public void StoreMessage(string msg)
    {
        storedMsg = msg;
    }

}
