using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StoreReceiptPrinter : MonoBehaviour
{
    //ex. [ITEM_NAME] x[BUY_AMOUNT]
    public TextMeshProUGUI productName;
    //ex. [ITEM_PRICE * BUY_AMOUNT]|SEK
    public TextMeshProUGUI productPrice;
    //ex. Bulk Discount: -5%
    public TextMeshProUGUI discountText;

    //ex. Total: [ITEM_PRICE * BULK_DISCOUNT]|SEK
    public TextMeshProUGUI totalPriceText;
    //ex. Remaining Money: [PLAYER_MONEY - TOTAL_COST]|SEK
    public TextMeshProUGUI remainingCashText;

    private void Start()
    {
        PopulateReciept("Test", 12, 40, 1200);
    }

    public void PopulateReciept(string itemTitle, int amount, int msrp, int playerMoney)
    {
        productName.text = itemTitle + " x" + amount.ToString();
        productPrice.text = (msrp * amount) + "|SEK";

        discountText.transform.parent.gameObject.SetActive((amount >= 5));

        //Discount is -5% at x5, -10% at x10, -15% at x15 and -20% at x20
        int discount = 0;
        int amountStack = amount;
        while (amountStack >= 5)
        {
            amountStack -= 5;
            discount++;
        }

        discountText.text = $"Bulk Discount: -{discount * 5}%";

        int totalPrice = Mathf.RoundToInt((amount * msrp) * (1.0f - ((discount * 5) / 100.0f)));

        totalPriceText.text = "Total: " + totalPrice + "|SEK";
        remainingCashText.text = "Remaining Money: " + (playerMoney - totalPrice) + "|SEK";
    }
}
