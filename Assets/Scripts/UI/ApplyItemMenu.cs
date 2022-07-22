using Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ApplyItemMenu : MonoBehaviour
{
    public ItemPartyCard[] partyCards;
    public EntityScriptable testEntity;
    public InputSystemUIInputModule uiInput;
    private bool dumbGate;
    private bool active;

    [Header("Background")]
    public Image background;
    public AnimationCurve fadeCurve;
    public Gradient backgroundGradient;


    [Header("Background Flair")]
    public RectTransform backFlair;
    public Vector2 minMaxRotZ;
    public AnimationCurve flairLerpCurve;

    private void Awake()
    {
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
    }
    private void Start()
    {
        HideMenu();
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

    public void ListenForMessage(string msg)
    {
        switch (msg)
        {
            case "_openItemMenu":
                break;
        }
    }

    public void SubmitAction()
    {
        if (!active) return;

        for (int i = 0; i < partyCards.Length; i++)
        {
            if (partyCards[i].hovering)
            {
                partyCards[i].OnCursorClick();
            }
        }
    }

    public void UseItemOnEntity(EntityScriptable entity)
    {
        //Depending on item type, apply it to the entity and then refresh the item menu before closing the applymenu
    }


    private IEnumerable IEIntro()
    {
        for (int i = 0; i < partyCards.Length; i++)
        {
            partyCards[i].UpdateEntity(testEntity);
            partyCards[i].onPress.AddListener(UseItemOnEntity);
        }

        float timer = 0;
        float maxTime = Mathf.Max(fadeCurve.keys[fadeCurve.length - 1].time, flairLerpCurve.keys[fadeCurve.length - 1].time);

        while (timer < maxTime)
        {
            background.color = backgroundGradient.Evaluate(fadeCurve.Evaluate(timer));

            Vector2 flairSize = backFlair.sizeDelta;
            flairSize.y = flairLerpCurve.Evaluate(timer);
            backFlair.sizeDelta = flairSize;

            backFlair.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minMaxRotZ.x, minMaxRotZ.y, timer / maxTime));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        background.color = backgroundGradient.Evaluate(fadeCurve.Evaluate(maxTime));
        Vector2 flairSize2 = backFlair.sizeDelta;
        flairSize2.y = flairLerpCurve.Evaluate(maxTime);
        backFlair.sizeDelta = flairSize2;
        backFlair.rotation = Quaternion.Euler(0, 0, minMaxRotZ.y);
        active = true;

    }

    private void HideMenu()
    {
        Color col = background.color;
        col.a = 0;
        background.color = col;
    }
}
