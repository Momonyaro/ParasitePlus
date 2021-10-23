using UnityEngine;

namespace BattleSystem.UI
{
    public class DamageEffectUI : MonoBehaviour
    {
        public GameObject damageEffectPrefab;
        public GameObject healEffectPrefab;
    
        public void CreateDamageSpatter(Vector2 canvasPos, int damageNumber, bool crit, bool weak, bool resist)
        {
            GameObject effect = (damageNumber >= 0) ? Instantiate(damageEffectPrefab, transform) : Instantiate(healEffectPrefab, transform);
            
            RectTransform imageTransform = effect.GetComponent<RectTransform>();
            imageTransform.position = canvasPos;

            BloodSpatterSprayer comp = effect.transform.GetChild(0).GetComponent<BloodSpatterSprayer>();

            comp.damageText.color = (crit) ? Color.red : Color.white;

            Vector2 sizeDelta = comp.damageText.rectTransform.sizeDelta;
            comp.damageText.rectTransform.sizeDelta = new Vector2(sizeDelta.x, (crit) ? 180 : 140);
            
            comp.damageText.text = Mathf.Abs(damageNumber).ToString();
            
            if (comp.weakText != null)
                comp.weakText.gameObject.SetActive(weak);
            if (comp.resistText != null)
                comp.resistText.gameObject.SetActive(resist);
            
            if (damageNumber == 0)
                comp.damageText.text = "Miss";
        }

        public void CreateAbilityEffect(Vector2 canvasPos, string effectRef)
        {
            GameObject effect = Instantiate(Resources.Load<GameObject>($"Effects/{effectRef}"), transform);
            RectTransform rect = effect.GetComponent<RectTransform>();

            rect.position = canvasPos;

            Debug.Log(effect.name);

        }
    }
}
