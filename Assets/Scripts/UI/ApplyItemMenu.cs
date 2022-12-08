using Scriptables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class ApplyItemMenu : MonoBehaviour
{
    public ItemPartyCard[] partyCards;
    public InputSystemUIInputModule uiInput;
    public Items.Item storedItem;
    private bool dumbGate;
    public bool active;

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
            case "_closeSelectMember":
                StopCoroutine(IEIntro().GetEnumerator());
                StopCoroutine(IEOutro().GetEnumerator());
                StartCoroutine(IEOutro().GetEnumerator());
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

    public void RevealMenu(Items.Item item)
    {
        storedItem = item;

        StopCoroutine(IEIntro().GetEnumerator());
        StopCoroutine(IEOutro().GetEnumerator());
        StartCoroutine(IEIntro().GetEnumerator());
    }

    public void UseItemOnEntity(EntityScriptable entity)
    {
        //Depending on item type, apply it to the entity and then refresh the item menu before closing the applymenu

        ItemMenu iMenu = FindObjectOfType<ItemMenu>();

        if (storedItem.itemAbility != null)
        {
            string sfxRef = storedItem.itemAbility.abilitySoundEffect;
            if (!sfxRef.Equals(""))
            {
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack(sfxRef, out bool success);
            }    
        }

        switch (storedItem.type)
        {
            case Items.ItemType.AID:
                UseHealing(ref entity);
                break;
            default:
                break;
        }

        iMenu.OnItemUsed(storedItem.guid);
        storedItem = null;

        StopCoroutine(IEIntro().GetEnumerator());
        StopCoroutine(IEOutro().GetEnumerator());
        StartCoroutine(IEOutro().GetEnumerator());
    }

    private void UseHealing(ref EntityScriptable entity)
    {
        Vector2Int hp = entity.GetEntityHP();
        Vector2Int ap = entity.GetEntityAP();

        Debug.Log("Healing for " + storedItem.itemAbility.abilityDamage.x + " damage.");

        hp.x -= storedItem.itemAbility.abilityDamage.x;
        hp.x = Mathf.Min(hp.x, hp.y); 
        ap.x -= storedItem.itemAbility.abilityDamage.y;
        ap.x = Mathf.Min(ap.x, ap.y);

        entity.SetEntityHP(hp);
        entity.SetEntityAP(ap);
    }


    private IEnumerable IEIntro()
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();

        List<EntityScriptable> activeParty = mapManager.currentSlimData.partyField.Where(e => e.inParty).ToList();
        while (activeParty.Count < partyCards.Length)
        {
            activeParty.Add(null);
        }

        for (int i = 0; i < partyCards.Length; i++)
        {
            partyCards[i].UpdateEntity(activeParty[i]);
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

    private IEnumerable IEOutro()
    {
        for (int i = 0; i < partyCards.Length; i++)
        {
            partyCards[i].onPress.RemoveAllListeners();
            partyCards[i].active = false;
        }

        float timer = Mathf.Max(fadeCurve.keys[fadeCurve.length - 1].time, flairLerpCurve.keys[fadeCurve.length - 1].time);
        float maxTime = 0;

        while (timer > maxTime)
        {
            background.color = backgroundGradient.Evaluate(fadeCurve.Evaluate(timer));

            Vector2 flairSize = backFlair.sizeDelta;
            flairSize.y = flairLerpCurve.Evaluate(timer);
            backFlair.sizeDelta = flairSize;

            backFlair.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(minMaxRotZ.x, minMaxRotZ.y, timer / maxTime));

            timer -= Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }

        background.color = backgroundGradient.Evaluate(fadeCurve.Evaluate(maxTime));
        Vector2 flairSize2 = backFlair.sizeDelta;
        flairSize2.y = flairLerpCurve.Evaluate(maxTime);
        backFlair.sizeDelta = flairSize2;
        backFlair.rotation = Quaternion.Euler(0, 0, minMaxRotZ.x);

        CORE.UIManager.Instance.onUIMessage.Invoke("_unlockItemMenu");
        active = false;
    }

    private void HideMenu()
    {
        Color col = background.color;
        col.a = 0;
        background.color = col;
        active = false;
    }
}
