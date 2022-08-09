using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class StoreInterface : MonoBehaviour
{
    public Image background;
    public AnimationCurve lerpCurve;
    public ItemBtn[] itemButtons = new ItemBtn[0];
    public ItemBtn nextPageButton;
    public ItemBtn prevPageButton;
    public ItemBtn returnButton;
    public Items.ItemDatabase storeInventory;
    private int currentIndex = 0;
    public TMPro.TextMeshProUGUI noItemText;
    public TMPro.TextMeshProUGUI pageInfoText;
    public InputSystemUIInputModule uiInput;

    public UnityEvent onClose = new UnityEvent();

    private List<Items.Item> fetchedInventory = new List<Items.Item>();
    private List<Items.Item> lastPlayerInventory = new List<Items.Item>();
    private bool dumbGate = false;
    private bool visible = false;
    private bool buyMode = true;

    public List<Items.Item> PlayerInventory => lastPlayerInventory;

    private void Awake()
    {
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
        background.fillAmount = 0;
        foreach (ItemBtn itemBtn in itemButtons)
        {
            itemBtn.active = false;
        }
        prevPageButton.active = false;
        nextPageButton.active = false;
    }

    private void OnEnable()
    {
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed += OnMouseSubmit;
        ui.FindAction("Submit").started += OnSubmit;
    }

    private void OnDisable()
    {
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed -= OnMouseSubmit;
        ui.FindAction("Submit").started -= OnSubmit;
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
        if (!visible) return;

        if (nextPageButton.hovering)
            nextPageButton.OnCursorClick();

        if (prevPageButton.hovering)
            prevPageButton.OnCursorClick();

        if (returnButton.hovering)
            returnButton.OnCursorClick();

        for (int i = 0; i < itemButtons.Length; i++)
        {
            if (itemButtons[i].hovering)
                itemButtons[i].OnCursorClick();
        }
    }

    public void OpenBuyMenu()
    {
        SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_submit", out bool success);
        buyMode = true;

        fetchedInventory = 
            new List<Items.Item>(storeInventory.GetAllStoreItems(StoreManager.GetLastPlayerLevel(out bool success2)));
        lastPlayerInventory = FindObjectOfType<StoreManager>().currentSlim.inventory;

        StopAllCoroutines();
        StartCoroutine(IEOpen());
    }

    public void OpenSellMenu()
    {
        buyMode = false;

        fetchedInventory = 
            new List<Items.Item>(FindObjectOfType<StoreManager>().currentSlim.inventory);
        lastPlayerInventory = new List<Items.Item>(fetchedInventory);

        StopAllCoroutines();
        StartCoroutine(IEOpen());
    }

    public void CloseMenu()
    {
        returnButton.active = false;
        SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_cancel", out bool success);
        ItemDetailWindow.Instance.HideDetailWindow();
        StopAllCoroutines();
        StartCoroutine(IEClose());
    }

    public void PopulateList()
    {
        int currentPageNumber = 1;
        int totalPageNumber = 1;

        if (fetchedInventory.Count != 0 && currentIndex != 0)
            currentPageNumber = Mathf.CeilToInt((currentIndex + itemButtons.Length) / (float)itemButtons.Length);

        if (fetchedInventory.Count != 0)
            totalPageNumber = Mathf.CeilToInt(fetchedInventory.Count / (float)itemButtons.Length);

        returnButton.active = true;

        noItemText.gameObject.SetActive((fetchedInventory.Count == 0));
        lastPlayerInventory = FindObjectOfType<StoreManager>().currentSlim.inventory;


        //int currentPageNumber = Mathf.CeilToInt((lastInventory.Count != 0) ? currentIndex / (float)lastInventory.Count : 1);
        //int totalPageNumber = Mathf.CeilToInt((lastInventory.Count != 0) ? lastInventory.Count / (float)listSlots.Count : 1);

        pageInfoText.text = $"{currentPageNumber}/{totalPageNumber}";

        nextPageButton.gameObject.SetActive((totalPageNumber > 1));
        nextPageButton.StoreMessage("_nextItemPage");
        nextPageButton.itemDescription = "Go to the next page of items.";
        prevPageButton.gameObject.SetActive((totalPageNumber > 1));
        prevPageButton.StoreMessage("_lastItemPage");
        prevPageButton.itemDescription = "Go to the previous page of items.";

        for (int i = 0; i < itemButtons.Length; i++)
        {
            bool itemExists = (fetchedInventory.Count > currentIndex + i);

            itemButtons[i].onPress.RemoveAllListeners();
            itemButtons[i].gameObject.SetActive(itemExists);
            itemButtons[i].active = itemExists;

            if (itemExists)
            {
                Items.Item current = fetchedInventory[currentIndex + i];

                Items.Item match = null;
                for (int j = 0; j < lastPlayerInventory.Count; j++)
                {
                    if (lastPlayerInventory[j].guid.Equals(current.guid))
                    {
                        Debug.Log("Found Match");
                        match = lastPlayerInventory[j];
                        break;
                    }
                }

                string multipleText = $"<color=lightblue>0</color> ";

                if (match != null)
                {
                    multipleText = (match.stackable) ? $"<color=lightblue>{match.StackSize.x}</color> " : "<color=red>MAX </color>";
                }

                string title = $"{multipleText}{current.name}";
                string extra = $"<color=lightblue>{current.msrp}|SEK</color> ";

                itemButtons[i].SetButtonData(title, extra, current.guid, current.description);
                itemButtons[i].AddClickListener(ParseItem);
                //itemButtons[i].onPress.AddListener(TestEvent);
            }

        }
    }

    public void ParseItem(string itemGuid)
    {
        Debug.Log(itemGuid);
        Items.Item item = null;
        for (int i = 0; i < fetchedInventory.Count; i++)
        {
            if (fetchedInventory[i].guid.Equals(itemGuid))
            {
                item = fetchedInventory[i];
                break;
            }
        }

        if (item == null) return;

        Items.Item match = null;
        for (int j = 0; j < lastPlayerInventory.Count; j++)
        {
            if (lastPlayerInventory[j].guid.Equals(item.guid))
            {
                match = lastPlayerInventory[j];
                break;
            }
        }

        int invAmount = (match != null) ? match.StackSize.x : 0;

        if (item.msrp > FindObjectOfType<StoreManager>().currentSlim.wallet)
            return;

        SetMenuVisibility(false);
        SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_submit", out bool success);
        FindObjectOfType<PaymentMenu>().PrepareBuyItem(item, invAmount, FindObjectOfType<StoreManager>().currentSlim.wallet);
    }

    public void SetMenuVisibility(bool visibility)
    {
        visible = visibility;

        if (visible)
            PopulateList();
    }


    private IEnumerator IEOpen()
    {
        StoreManager manager = FindObjectOfType<StoreManager>();

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        PopulateList();

        while (timer < maxTimer)
        {
            background.fillAmount = lerpCurve.Evaluate(timer);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        background.fillAmount = lerpCurve.Evaluate(maxTimer);
        visible = true;

        yield break;
    }

    private IEnumerator IEClose()
    {
        StoreManager manager = FindObjectOfType<StoreManager>();

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        while (timer < maxTimer)
        {
            background.fillAmount = lerpCurve.Evaluate(maxTimer - timer);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        background.fillAmount = lerpCurve.Evaluate(0);
        visible = false;
        onClose.Invoke();

        yield break;
    }
}
