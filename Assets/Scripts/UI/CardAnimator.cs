using System.Collections;
using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class CardAnimator : MonoBehaviour
{
    public RectTransform cardTransform;
    public float closedHeight = 0;
    public float openHeight = 350;
    public float lerpSpeed = 0.5f;
    public UIButton btnComponent;

    [SerializeField] private Image charSprRenderer;
    [SerializeField] private TextMeshProUGUI hp, maxHp;
    [SerializeField] private Image hpBar;
    [SerializeField] private TextMeshProUGUI ap, maxAp;
    [SerializeField] private Image apBar;


    private void Start()
    {
        Vector2 size = cardTransform.sizeDelta;
        size.y = btnComponent.active ? openHeight : closedHeight;
        cardTransform.sizeDelta = size;
    }

    private void Update()
    {
        Vector2 size = cardTransform.sizeDelta;
        size.y = btnComponent.active ? openHeight : closedHeight;
        cardTransform.sizeDelta = Vector2.Lerp(cardTransform.sizeDelta, size, lerpSpeed);
    }

    public void SetValues(Sprite character, int hp, int maxHp, int ap, int maxAp)
    {
        charSprRenderer.sprite = character;
        this.hp.text = hp.ToString();
        this.maxHp.text = maxHp.ToString();
        this.ap.text = ap.ToString();
        this.maxAp.text = maxAp.ToString();
        hpBar.fillAmount = hp / (float)maxHp;
        apBar.fillAmount = ap / (float)maxAp;
    }
}
