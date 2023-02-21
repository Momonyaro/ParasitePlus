using Scriptables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyStatusMenu : MonoBehaviour
{
    public Image portrait;
    public TextMeshProUGUI hp, maxHp;
    public Image hpBar;
    public TextMeshProUGUI ap, maxAp;
    public Image apBar;
    public TextMeshProUGUI tName;
    public TextMeshProUGUI tLvl;
    public TextMeshProUGUI tWeapon;

    public Image[] weaknesses = new Image[0];

    public Sprite buffIco, weakIco, normIco;

    [Header("Portraits")]
    public Sprite playerSprite;
    public Sprite gummoSprite;
    public Sprite sandraSprite;
    public Sprite siveSprite;

    public void SetData(string id)
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();
        List<EntityScriptable> party = mapManager.currentSlimData.partyField.ToList();
        var found = party.Find((i) => { return i != null && i.entityId == id; });

        portrait.sprite = GetSprite(found.entityId);

        var hp = found.GetEntityHP();
        this.hp.text = hp.x.ToString(); maxHp.text = hp.y.ToString();
        hpBar.fillAmount = hp.x / (float)hp.y;

        var ap = found.GetEntityAP();
        this.ap.text = ap.x.ToString(); maxAp.text = ap.y.ToString();
        apBar.fillAmount = ap.x / (float)ap.y;

        tName.text = found.entityName;
        tLvl.text = found.entityLevel.ToString();
        tWeapon.text = found.weapon;

        for (int i = 0; i < found.weaknesses.Length; i++)
        {
            var iconPack = GetWeaknessIcon(found.weaknesses[i]);
            weaknesses[i].sprite = iconPack.icon;
            weaknesses[i].color = iconPack.color;
        }
    }

    private Sprite GetSprite(string id)
    {
        switch (id)
        {
            case "_player": return playerSprite;
            case "_gummo": return gummoSprite;
            case "_sandra": return sandraSprite;
            case "_sive": return siveSprite;
            default: return null;
        }
    }

    private (Color color, Sprite icon) GetWeaknessIcon(float weakness)
    {
        Color strong = new Color(0.04f, 1, 0); //Green
        Color weak = new Color(1, 0, 0.04f);   //Red

        Color selectedCol = Color.white;
        Sprite selectedSpr = normIco;
        if (weakness > 1.1f) { selectedCol = weak; selectedSpr = weakIco; }
        if (weakness < 0.9f) { selectedCol = strong; selectedSpr = buffIco; }

        return (selectedCol, selectedSpr);
    }
}
