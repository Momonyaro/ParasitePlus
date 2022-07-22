using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemDetailWindow : MonoBehaviour
{

    public static ItemDetailWindow Instance;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image background;

    private void Awake()
    {
        ItemDetailWindow.Instance = this;
    }

    private void Start()
    {
        HideDetailWindow();
    }

    public void CreateDetailWindow(string title, string description)
    {
        background.gameObject.SetActive(true);
        titleText.text = title;
        descriptionText.text = description;
    }

    public void HideDetailWindow()
    {
        background.gameObject.SetActive(false);
    }
}
