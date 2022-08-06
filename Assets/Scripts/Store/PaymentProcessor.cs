using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PaymentProcessor : MonoBehaviour
{
    public TextMeshProUGUI itemTitle;
    public TextMeshProUGUI itemDescription;
    public TextMeshProUGUI inInventoryText;

    public TextMeshProUGUI itemAmountText;
    public ItemBtn sub10Button;
    public ItemBtn subButton;
    public ItemBtn addButton;
    public ItemBtn add10Button;

    public ItemBtn purchaseButton;
    public ItemBtn returnButton;
    public InputSystemUIInputModule uiInput;

    private Items.Item item;
    private int amount = 1;
    private int inventoryAmount = 0;
    private int maxAmount = 1;
    private int maxPurchasable = 1;
    private int currentMoney = 0;
    private bool dumbGate;
    private bool visible = false;

    private void Awake()
    {
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
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

    public void SetMenuInteractable(bool interactable)
    {
        visible = interactable;

        sub10Button.active = interactable;
        subButton.active = interactable;
        addButton.active = interactable;
        add10Button.active = interactable;
        returnButton.active = interactable;

        purchaseButton.active = interactable;
    }

    public void PopulateMenu(Items.Item toPurchase, int inInventory, int maxStack, int currentMoney)
    {
        item = new Items.Item(toPurchase);
        itemTitle.text = toPurchase.name;
        itemDescription.text = toPurchase.description;

        amount = 1;
        inventoryAmount = inInventory;
        this.currentMoney = currentMoney;
        maxAmount = maxStack;
        maxPurchasable = maxAmount - inventoryAmount;

        inInventoryText.text = $"IN INVENTORY: {inventoryAmount}/{maxAmount}";

        itemAmountText.text = amount.ToString();
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

    public void ChangeBuyAmount(int delta)
    {
        int absDelta = Mathf.Abs(delta);
        for (int i = 0; i < absDelta; i++)
        {
            int futureAmount = amount + Mathf.RoundToInt(Mathf.Sign(delta));
            futureAmount = Mathf.Clamp(futureAmount, 1, maxPurchasable);
            int futurePrice = CalculatePrice(item.msrp, futureAmount);
            if (futurePrice > currentMoney || futurePrice < 0) break;

            amount = futureAmount;
        }

        SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_storeAddSub", out bool success);

        UpdateWindow();
    }

    public void BuySelectedItem()
    {
        StoreManager manager = FindObjectOfType<StoreManager>();
        StoreInterface sInterface = FindObjectOfType<StoreInterface>();

        bool foundInInventory = false;
        for (int i = 0; i < manager.currentSlim.inventory.Count; i++)
        {
            if (manager.currentSlim.inventory[i].guid.Equals(item.guid))
            {
                foundInInventory = true;
                manager.currentSlim.inventory[i].StackSize.x = amount;
                break;
            }
        }

        if (!foundInInventory)
        {
            item.StackSize.x = amount;
            manager.currentSlim.inventory.Add(item);
        }

        int totalCost = CalculatePrice(item.msrp, amount);
        int remainingMoney = currentMoney - totalCost;

        manager.currentSlim.wallet = remainingMoney;

        FindObjectOfType<StoreGreetingMenu>().WriteWalletText(remainingMoney);
    }

    public void UpdateWindow()
    {
        inInventoryText.text = $"IN INVENTORY: {inventoryAmount}/{maxAmount}";
        itemAmountText.text = amount.ToString();
        FindObjectOfType<PaymentMenu>().UpdateReciept(item, amount);
    }

    public void SubmitAction()
    {
        if (!visible) return;

        if (sub10Button.hovering)
            sub10Button.OnCursorClick();
        if (subButton.hovering)
            subButton.OnCursorClick();
        if (addButton.hovering)
            addButton.OnCursorClick();
        if (add10Button.hovering)
            add10Button.OnCursorClick();

        if (purchaseButton.hovering)
            purchaseButton.OnCursorClick();
        if (returnButton.hovering)
            returnButton.OnCursorClick();
    }

    private int CalculatePrice(int msrp, int amount)
    {
        //Discount is -5% at x5, -10% at x10, -15% at x15 and -20% at x20
        int discount = 0;
        int amountStack = amount;
        while (amountStack >= 5)
        {
            amountStack -= 5;
            discount++;
        }

        int totalPrice = Mathf.RoundToInt((amount * msrp) * (1.0f - ((discount * 5) / 100.0f)));

        return totalPrice;
    }
}
