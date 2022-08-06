using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class StoreGreetingMenu : MonoBehaviour
{
    public Image background;
    public AnimationCurve lerpCurve;
    public ShopModeButton buyBtn;
    public ShopModeButton sellBtn;
    public ItemBtn leaveBtn;
    public GameObject wallet;
    public TextMeshProUGUI walletText;
    public InputSystemUIInputModule uiInput;

    public bool visible = false;

    private bool dumbGate;

    //Replace $ with money amount
    private const string WalletText = "WALLET: $|SEK";

    private void Awake()
    {
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
        background.fillAmount = 0;
        buyBtn.gameObject.SetActive(false);
        sellBtn.gameObject.SetActive(false);
        leaveBtn.gameObject.SetActive(false);
        wallet.SetActive(false);
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

    public void GreetPlayer()
    {
        StartCoroutine(IEGreetings());
    }

    private IEnumerator IEGreetings()
    {
        StoreManager manager = FindObjectOfType<StoreManager>();
        wallet.SetActive(true);
        WriteWalletText(manager.currentSlim.wallet);

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        while (timer < maxTimer)
        {
            background.fillAmount = lerpCurve.Evaluate(timer);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        background.fillAmount = lerpCurve.Evaluate(maxTimer);

        UnlockMenu();

        yield break;
    }

    public void WriteWalletText(int money)
    {
        walletText.text = WalletText.Replace("$", money.ToString());
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

        if (buyBtn.IsHovering)
            buyBtn.OnCursorClick();

        if (sellBtn.IsHovering)
            sellBtn.OnCursorClick();

        if (leaveBtn.hovering)
            leaveBtn.OnCursorClick();
    }

    public void LockMenu()
    {
        visible = false;
        buyBtn.gameObject.SetActive(false);
        buyBtn.active = false;
        sellBtn.gameObject.SetActive(false);
        buyBtn.active = false;
        leaveBtn.gameObject.SetActive(false);
        leaveBtn.active = false;
    }

    public void UnlockMenu()
    {
        visible = true;
        buyBtn.gameObject.SetActive(true);
        buyBtn.active = true;
        sellBtn.gameObject.SetActive(true);
        buyBtn.active = true;
        leaveBtn.gameObject.SetActive(true);
        leaveBtn.active = true; 
    }
}
