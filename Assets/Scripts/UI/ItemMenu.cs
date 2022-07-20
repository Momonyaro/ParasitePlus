using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemMenu : MonoBehaviour
{
    public List<ItemBtn> listSlots = new List<ItemBtn>();

    public GameObject pageInfoParent;
    public TextMeshProUGUI pageInfoText;

    public TextMeshProUGUI noItemText;

    public GameObject nextItemBtn;
    public GameObject prevItemBtn;

    private List<Items.Item> lastInventory = new List<Items.Item>();
    private int currentIndex = 0;


    private void OnEnable()
    {
        CORE.UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
    }

    private void OnDisable()
    {
        CORE.UIManager.Instance.onUIMessage.RemoveListener(ListenForMessage);
    }

    public void ListenForMessage(string msg)
    {
        switch (msg)
        {
            case "_openItemMenu":
                PopulateList();
                break;
        }
    }

    private void PopulateList()
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();
        currentIndex = 0;

        lastInventory = mapManager.currentSlimData.inventory;

        int currentPageNumber = 1;
        int totalPageNumber = 1;

        if (lastInventory.Count != 0 && currentIndex != 0)
            currentPageNumber = Mathf.CeilToInt(currentIndex / (float)lastInventory.Count);

        if (lastInventory.Count != 0)
            totalPageNumber = Mathf.CeilToInt(lastInventory.Count / (float)listSlots.Count);

        noItemText.gameObject.SetActive((lastInventory.Count == 0));


        //int currentPageNumber = Mathf.CeilToInt((lastInventory.Count != 0) ? currentIndex / (float)lastInventory.Count : 1);
        //int totalPageNumber = Mathf.CeilToInt((lastInventory.Count != 0) ? lastInventory.Count / (float)listSlots.Count : 1);

        pageInfoText.text = $"{currentPageNumber}/{totalPageNumber}";

        nextItemBtn.SetActive((totalPageNumber > 1));   
        prevItemBtn.SetActive((totalPageNumber > 1));

        for (int i = 0; i < listSlots.Count; i++)
        {
            bool itemExists = (lastInventory.Count > currentIndex + i);

            listSlots[i].gameObject.SetActive(itemExists);

            if (itemExists)
            {
                Items.Item current = lastInventory[currentIndex + i];

                string multipleText = current.StackSize.x > 1 ? $"<color=lightblue>{current.StackSize.x}x</color> " : "";

                string title = $"{multipleText}{current.name}";
                string extra = $"<color=lightblue>{current.type}</color> ";

                listSlots[i].SetButtonData(title, extra, current.guid);
            }

        }
    }
}
