using Scriptables;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemPartyCard : MonoBehaviour
{
    public RectTransform cardRect;
    public Image heldMember;
    public Image portraitFrame;
    public Image healthBar;
    public TextMeshProUGUI healthText;
    public Image actionBar;
    public TextMeshProUGUI actionText;
    public AnimationCurve introFillCurve;
    public bool active = false;
    public bool hovering = false;

    public EntityScriptable lastEntity;
    public UnityEvent<EntityScriptable> onPress;
    private MapController mController;

    [Header("Portraits")]
    public Sprite playerSprite;
    public Sprite gummoSprite;
    public Sprite sandraSprite;
    public Sprite siveSprite;


    private void Awake()
    {
        mController = FindObjectOfType<MapController>();
    }

    private void Update()
    {
        if (!active) return;

        bool inside = IsInsideRect(mController.GetCursorScreenPos);

        if (!hovering && inside)
        {
            OnCursorEnter();
        }
        else if (hovering && !inside)
            OnCursorExit();
    }

    //This won't work with gamepads, re-do
    public void OnCursorClick()
    {
        if (!active) return;
        onPress.Invoke(lastEntity);
    }

    public void OnCursorEnter()
    {
        heldMember.color = Color.white;
        hovering = true;
    }

    public void OnCursorExit()
    {
        Color color;
        ColorUtility.TryParseHtmlString("#001269", out color);
        heldMember.color = color;
        hovering = false;
    }

    public bool IsInsideRect(Vector2 pos)
    {
        Vector2 screenPos = cardRect.position;
        Vector2 screenSizeDelta = cardRect.sizeDelta;

        // screenpos +- sizedelta should give us the space that the image occupies in screen space.

        Rect screenRect = new Rect(screenPos - (screenSizeDelta * 0.66f * 0.5f) - new Vector2(0, screenSizeDelta.y * 0.25f), screenSizeDelta * 0.66f);

        return screenRect.Contains(pos);
    }

    public void UpdateEntity(EntityScriptable entity)
    {
        lastEntity = null;
        active = false;
        heldMember.gameObject.SetActive(entity != null && entity.inParty);

        if (entity == null || !entity.inParty)
        {
            return;
        }

        lastEntity = entity;
        active = true;

        Vector2Int hp = lastEntity.GetEntityHP();
        Vector2Int ap = lastEntity.GetEntityAP();
        SetEntityPortrait(lastEntity.entityId);
        SetHPValues(hp.x, hp.y);
        SetAPValues(ap.x, ap.y);

        StopCoroutine(IEIntroFill());
        StartCoroutine(IEIntroFill());

        IEnumerator IEIntroFill()
        {
            float timer = 0;
            float maxTime = introFillCurve.keys[introFillCurve.length - 1].time;

            while (timer < maxTime)
            {
                Vector2 size = cardRect.sizeDelta;
                size.y = introFillCurve.Evaluate(timer);
                cardRect.sizeDelta = size;

                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            Vector2 size2 = cardRect.sizeDelta;
            size2.y = introFillCurve.Evaluate(maxTime);
            cardRect.sizeDelta = size2;

            yield break;
        }
    }

    private void SetEntityPortrait(string entityId)
    {
        switch (entityId)
        {
            case "_player":
                portraitFrame.sprite = playerSprite;
                break;
            case "_gummo":
                portraitFrame.sprite = gummoSprite;
                break;
            case "_sandra":
                portraitFrame.sprite = sandraSprite;
                break;
            case "_sive":
                portraitFrame.sprite = siveSprite;
                break;
            default:
                portraitFrame.sprite = null;
                break;
        }
    }

    private void SetHPValues(int currentHp, int maxHp)
    {
        healthText.text = $"HP: {currentHp}/{maxHp}";
        healthBar.fillAmount = currentHp / (float)maxHp; 
    }

    private void SetAPValues(int currentAp, int maxAp)
    {
        actionText.text = $"AP: {currentAp}/{maxAp}";
        actionBar.fillAmount = currentAp / (float)maxAp;
    }
}
