using CORE;
using Scriptables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class PauseCardUpdater : MonoBehaviour
{
    public CardAnimator[] partyCards = new CardAnimator[0];

    [Header("Portraits")]
    public Sprite playerSprite;
    public Sprite gummoSprite;
    public Sprite sandraSprite;
    public Sprite siveSprite;

    public void UpdateEntities()
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();
        var statusMenu = FindObjectOfType<PartyStatusMenu>();

        List <EntityScriptable> activeParty = mapManager.currentSlimData.partyField.Where(e => e != null && e.inParty).ToList();

        for (int i = 0; i < partyCards.Length; i++)
        {
            partyCards[i].gameObject.SetActive(false);
            var btn = partyCards[i].GetComponent<UIButton>();
            btn.onClick.RemoveAllListeners();
        }

        for (int i = 0; i < activeParty.Count; i++)
        {
            var e = activeParty[i];
            partyCards[i].gameObject.SetActive(true);
            partyCards[i].SetValues(GetSprite(e.entityId), e.GetEntityHP().x, e.GetEntityHP().y, e.GetEntityAP().x, e.GetEntityAP().y);
            var btn = partyCards[i].GetComponent<UIButton>();
            btn.onClick.AddListener(() => btn.SendMessage());
            btn.onClick.AddListener(() => statusMenu.SetData(e.entityId));
        }
    }

    private Sprite GetSprite(string id)
    {
        switch(id)
        {
            case "_player": return playerSprite;
            case "_gummo": return gummoSprite;
            case "_sandra": return sandraSprite;
            case "_sive": return siveSprite;
            default: return null;
        }
    }
}
