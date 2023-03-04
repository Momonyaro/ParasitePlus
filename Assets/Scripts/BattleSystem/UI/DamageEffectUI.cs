using UnityEngine;

namespace BattleSystem.UI
{
    public class DamageEffectUI : MonoBehaviour
    {
        public GameObject damageEffectPrefab;
        public GameObject healEffectPrefab;
    
        public void CreateDamageSpatter(Vector2 canvasPos, int hpDamage, int apDamage, bool crit, bool weak, bool resist, bool showDamageNum = true)
        {
            GameObject effect = (hpDamage >= 0) ? Instantiate(damageEffectPrefab, transform) : Instantiate(healEffectPrefab, transform);
            
            RectTransform imageTransform = effect.GetComponent<RectTransform>();
            imageTransform.position = canvasPos;

            BloodSpatterSprayer comp = effect.transform.GetChild(0).GetComponent<BloodSpatterSprayer>();

            comp.hpDamageText.color = (crit) ? comp.hpDamColors.Evaluate(1) : comp.hpDamColors.Evaluate(0);
            comp.apDamageText.color = (crit) ? comp.apDamColors.Evaluate(1) : comp.apDamColors.Evaluate(0);

            Vector2 sizeDelta = comp.hpDamageText.rectTransform.sizeDelta;
            comp.hpDamageText.rectTransform.sizeDelta = new Vector2(sizeDelta.x, (crit) ? 180 : 140);

            if (crit)
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_critShatter", out bool success);

            if (hpDamage > 0)
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_bloodSpatter", out bool success);
            else if (hpDamage < 0)
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_healSpatter", out bool success);

            comp.hpDamageText.text = Mathf.Abs(hpDamage).ToString();
            comp.apDamageText.text = Mathf.Abs(apDamage).ToString();
            
            if (comp.weakText != null)
                comp.weakText.gameObject.SetActive(weak);
            if (comp.resistText != null)
                comp.resistText.gameObject.SetActive(resist);
            
            if (hpDamage == 0 && apDamage == 0)
                comp.hpDamageText.text = "Miss";
            else if (hpDamage == 0)
                comp.hpDamageText.text = "";

            if (apDamage == 0)
                comp.apDamageText.text = "";

            if (!showDamageNum)
            {
                comp.hpDamageText.text = "";
                comp.apDamageText.text = "";
            }
        }

        public void CreateAbilityEffect(Vector2 canvasPos, string effectRef)
        {
            GameObject effect = Instantiate(Resources.Load<GameObject>($"Effects/{effectRef}"), transform);
            RectTransform rect = effect.GetComponent<RectTransform>();

            rect.position = canvasPos;

        }
    }
}
