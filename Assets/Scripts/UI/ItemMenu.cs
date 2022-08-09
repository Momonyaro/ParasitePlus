using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class ItemMenu : MonoBehaviour
{
    public List<ItemBtn> listSlots = new List<ItemBtn>();

    public GameObject pageInfoParent;
    public TextMeshProUGUI pageInfoText;
    public ApplyItemMenu ApplyItemMenu;

    public TextMeshProUGUI noItemText;

    public ItemBtn nextItemBtn;
    public ItemBtn prevItemBtn;

    private List<Items.Item> lastInventory = new List<Items.Item>();
    public InputSystemUIInputModule uiInput;
    private int currentIndex = 0;
    private bool dumbGate = false;
    public bool active = false;

    private void Awake()
    {
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
    }

    private void OnEnable()
    {
        CORE.UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed += OnMouseSubmit;
        ui.FindAction("Submit").started += OnSubmit;
    }

    private void OnDisable()
    {
        CORE.UIManager.Instance.onUIMessage.RemoveListener(ListenForMessage);
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed -= OnMouseSubmit;
        ui.FindAction("Submit").started -= OnSubmit;
    }

    public void ListenForMessage(string msg)
    {
        switch (msg)
        {
            case "_openItemMenu":
                currentIndex = 0;
                active = true;
                prevItemBtn.active = true;
                nextItemBtn.active = true;
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_openItems", out bool success);
                PopulateList();
                break;
            case "_closeItemMenu":
                for (int i = 0; i < listSlots.Count; i++)
                {
                    listSlots[i].gameObject.SetActive(false);
                }
                prevItemBtn.active = false;
                nextItemBtn.active = false;
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_closeItems", out bool success2);
                Debug.Log("closeitems: [" + success2 + "]");
                active = false;
                break;
            case "_lastItemPage":
                currentIndex = Mathf.Max(0, currentIndex - listSlots.Count);
                PopulateList();
                break;
            case "_nextItemPage":
                if (currentIndex + listSlots.Count < lastInventory.Count)
                    currentIndex += listSlots.Count;
                PopulateList();
                break;
            case "_unlockItemMenu":
                active = true;
                break;
        }
    }

    private void PopulateList()
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();

        //Run this through type sorting later. First by type, then by alphabetic
        lastInventory = mapManager.currentSlimData.inventory;

        int currentPageNumber = 1;
        int totalPageNumber = 1;

        if (lastInventory.Count != 0 && currentIndex != 0)
            currentPageNumber = Mathf.CeilToInt((currentIndex + listSlots.Count) / (float)listSlots.Count);

        if (lastInventory.Count != 0)
            totalPageNumber = Mathf.CeilToInt(lastInventory.Count / (float)listSlots.Count);

        noItemText.gameObject.SetActive((lastInventory.Count == 0));


        //int currentPageNumber = Mathf.CeilToInt((lastInventory.Count != 0) ? currentIndex / (float)lastInventory.Count : 1);
        //int totalPageNumber = Mathf.CeilToInt((lastInventory.Count != 0) ? lastInventory.Count / (float)listSlots.Count : 1);

        pageInfoText.text = $"{currentPageNumber}/{totalPageNumber}";

        nextItemBtn.gameObject.SetActive((totalPageNumber > 1));
        nextItemBtn.StoreMessage("_nextItemPage");
        nextItemBtn.itemDescription = "Go to the next page of items.";
        prevItemBtn.gameObject.SetActive((totalPageNumber > 1));
        prevItemBtn.StoreMessage("_lastItemPage");
        prevItemBtn.itemDescription = "Go to the previous page of items.";

        for (int i = 0; i < listSlots.Count; i++)
        {
            bool itemExists = (lastInventory.Count > currentIndex + i);

            listSlots[i].onPress.RemoveAllListeners();
            listSlots[i].gameObject.SetActive(itemExists);
            listSlots[i].active = itemExists;

            if (itemExists)
            {
                Items.Item current = lastInventory[currentIndex + i];

                string multipleText = current.StackSize.x > 1 ? $"<color=lightblue>{current.StackSize.x}x</color> " : "";

                string title = $"{multipleText}{current.name}";
                string extra = $"<color=lightblue>{current.type}</color> ";

                listSlots[i].SetButtonData(title, extra, current.guid, current.description);
                listSlots[i].onPress.AddListener(ParseItem);
            }

        }
    }

    public void ParseItem(string itemGuid)
    {
        Debug.Log(itemGuid);

        Items.Item item = null;
        for (int i = 0; i < lastInventory.Count; i++)
        {
            if (lastInventory[i].guid.Equals(itemGuid))
            {
                item = lastInventory[i];
                break;
            }
        }

        if (item == null) return;

        switch (item.type)
        {
            case Items.ItemType.AID:
                ApplyItemMenu.RevealMenu(item);
                active = false;
                break;
            default:
                Debug.Log(item.name);
                break;
        }

    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        SubmitAction();
    }

    public void OnMouseSubmit(InputAction.CallbackContext context)
    {
        if (dumbGate)
        {
            dumbGate = false;
            return;
        }

        if (context.performed)
        {
            dumbGate = true;
        }

        SubmitAction();
    }

    public void SubmitAction()
    {
        if (!active || ApplyItemMenu.active) return;

        if (nextItemBtn.hovering)
            nextItemBtn.OnCursorClick();

        if (prevItemBtn.hovering)
            prevItemBtn.OnCursorClick();

        for (int i = 0; i < listSlots.Count; i++)
        {
            if (listSlots[i].hovering)
            {
                listSlots[i].OnCursorClick();
            }
        }
    }

    public void OnItemUsed(string itemId)
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();

        //Run this through type sorting later. First by type, then by alphabetic
        //lastInventory = mapManager.currentSlimData.inventory;
        foreach (var item in lastInventory)
        {
            if (item.guid.Equals(itemId))
            {
                if (item.stackable && item.StackSize.x > 1) { item.StackSize.x--; }
                else
                {
                    lastInventory.Remove(item);
                    break;
                }
            }
        }

        mapManager.currentSlimData.inventory = lastInventory;
        currentIndex = 0;
        PopulateList();
    }
}
