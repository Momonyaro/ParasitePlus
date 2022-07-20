using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemBtn : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI secondText;
    public Image background;

    public Color defaultCol = Color.white;
    public Color selectedCol = Color.white;
    public Color defaultTextCol = Color.black;
    public Color selectedTextCol = Color.black;

    public UnityEvent<string> onPress;

    private string storedMsg;

    public void OnPointerClick(PointerEventData eventData)
    {
        onPress.Invoke(storedMsg);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mainText.color = selectedTextCol;
        secondText.color = selectedTextCol;
        background.color = selectedCol;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mainText.color = defaultTextCol;
        secondText.color = defaultTextCol;
        background.color = defaultCol;
    }

    public void SetButtonData(string title, string extra, string toStore)
    {
        mainText.text = title;
        secondText.text = extra;
        StoreMessage(toStore);
    }

    public void StoreMessage(string msg)
    {
        storedMsg = msg;
    }

}
