using UnityEngine;

namespace BattleSystem.UI
{
    public class DamageEffectUI : MonoBehaviour
    {
        public GameObject damageEffectPrefab;
    
        public void CreateDamageSpatter(Vector2 canvasPos, int damageNumber, bool crit)
        {
            GameObject damageEffect = Instantiate(damageEffectPrefab, transform);
            
            RectTransform imageTransform = damageEffect.GetComponent<RectTransform>();
            imageTransform.position = canvasPos;

            BloodSpatterSprayer comp = damageEffect.transform.GetChild(0).GetComponent<BloodSpatterSprayer>();

            comp.damageText.color = (crit) ? Color.red : Color.white;

            Vector2 sizeDelta = comp.damageText.rectTransform.sizeDelta;
            comp.damageText.rectTransform.sizeDelta = new Vector2(sizeDelta.x, (crit) ? 180 : 140);
            
            comp.damageText.text = damageNumber.ToString();
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
