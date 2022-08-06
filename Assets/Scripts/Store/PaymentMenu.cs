using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PaymentMenu : MonoBehaviour
{
    public Image background;
    public Gradient backgroundGradient;
    public RectTransform paymentPanel;
    public AnimationCurve pPLerpCurve;
    public RectTransform receiptPanel;
    public AnimationCurve rPLerpCurve;
    public Vector3 rPStartPos;
    public Vector3 rPRestPos;
    public Vector3 rPEndPos;
    public RectTransform haloTransform;
    public AnimationCurve haloLerpCurve;

    public PaymentProcessor paymentProcessor;
    public StoreReceiptPrinter storeReceiptPrinter;
    public UnityEvent onClosed = new UnityEvent();

    private int playerWallet = 0;

    private Coroutine transition;

    private void Start()
    {
        background.color = backgroundGradient.Evaluate(0);

        Vector2 startDelta = paymentPanel.sizeDelta;
        startDelta.x = 0;
        paymentPanel.sizeDelta = startDelta;

        receiptPanel.anchoredPosition = rPStartPos;

        Vector2 haloStartDelta = haloTransform.sizeDelta;
        haloStartDelta.x = haloLerpCurve.Evaluate(0);
        haloTransform.sizeDelta = haloStartDelta;

        paymentProcessor.SetMenuInteractable(false);
    }

    public void OnPurchase()
    {
        PlayTransition(IEPurchase());
    }

    public void OnClose()
    {
        PlayTransition(IEClose());
    }

    public void PrepareBuyItem(Items.Item toBuy, int inventoryAmount, int playerMoney)
    {
        PlayTransition(IEOpen());
        playerWallet = playerMoney;
        paymentProcessor.PopulateMenu(toBuy, inventoryAmount, toBuy.StackSize.y, playerWallet);
        storeReceiptPrinter.PopulateReciept(toBuy.name, 1, toBuy.msrp, playerWallet);
    }

    public void UpdateReciept(Items.Item toBuy, int buyAmount)
    {
        storeReceiptPrinter.PopulateReciept(toBuy.name, buyAmount, toBuy.msrp, playerWallet);
    }

    private void PlayTransition(IEnumerator newTransition)
    {
        if (transition != null)
            StopCoroutine(transition);
        transition = StartCoroutine(newTransition);
    }

    private IEnumerator IEOpen()
    {
        float timer = 0;
        float maxTimer = Mathf.Max(pPLerpCurve.keys[pPLerpCurve.length - 1].time, rPLerpCurve.keys[pPLerpCurve.length - 1].time);

        while (timer < maxTimer)
        {
            background.color = backgroundGradient.Evaluate(timer / maxTimer);

            Vector2 sizeDelta = paymentPanel.sizeDelta;
            sizeDelta.x = pPLerpCurve.Evaluate(timer);
            paymentPanel.sizeDelta = sizeDelta;

            receiptPanel.anchoredPosition = Vector3.Lerp(rPStartPos, rPRestPos, rPLerpCurve.Evaluate(timer));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        background.color = backgroundGradient.Evaluate(1);
        Vector2 finalDelta = paymentPanel.sizeDelta;
        finalDelta.x = pPLerpCurve.Evaluate(maxTimer);
        paymentPanel.sizeDelta = finalDelta;
        receiptPanel.anchoredPosition = rPRestPos;

        paymentProcessor.SetMenuInteractable(true);

        yield break;
    }

    private IEnumerator IEClose()
    {
        float timer = 0;
        float maxTimer = Mathf.Max(pPLerpCurve.keys[pPLerpCurve.length - 1].time, rPLerpCurve.keys[pPLerpCurve.length - 1].time);

        paymentProcessor.SetMenuInteractable(false);

        while (timer < maxTimer)
        {
            float revertedTimer = maxTimer - timer;

            background.color = backgroundGradient.Evaluate(revertedTimer / maxTimer);

            Vector2 sizeDelta = paymentPanel.sizeDelta;
            sizeDelta.x = pPLerpCurve.Evaluate(revertedTimer);
            paymentPanel.sizeDelta = sizeDelta;

            receiptPanel.anchoredPosition = Vector3.Lerp(rPRestPos, rPEndPos, rPLerpCurve.Evaluate(timer));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        background.color = backgroundGradient.Evaluate(0);
        Vector2 finalDelta = paymentPanel.sizeDelta;
        finalDelta.x = pPLerpCurve.Evaluate(0);
        paymentPanel.sizeDelta = finalDelta;
        receiptPanel.anchoredPosition = rPEndPos;

        onClosed.Invoke();

        yield break;
    }

    private IEnumerator IEPurchase()
    {
        float timer = 0;
        float maxTimer = haloLerpCurve.keys[haloLerpCurve.length - 1].time;

        paymentProcessor.SetMenuInteractable(false);

        SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_storePurchase", out bool success);

        while (timer < maxTimer)
        {
            Vector2 sizeDelta = haloTransform.sizeDelta;
            sizeDelta.x = haloLerpCurve.Evaluate(timer);
            sizeDelta.y = haloLerpCurve.Evaluate(timer);
            haloTransform.sizeDelta = sizeDelta;

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Vector2 finalDelta = haloTransform.sizeDelta;
        finalDelta.x = haloLerpCurve.Evaluate(maxTimer);
        finalDelta.y = haloLerpCurve.Evaluate(maxTimer);
        haloTransform.sizeDelta = finalDelta;

        yield return new WaitForSeconds(0.1f);

        PlayTransition(IEClose());
    }
}
