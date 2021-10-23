using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BattleSystem.UI
{
    public class BloodSpatterSprayer : MonoBehaviour
    {
        public TextMeshProUGUI damageText;
        public TextMeshProUGUI weakText;
        public TextMeshProUGUI resistText;
        public float spatterRadius;
        public float spatterSize;
        public int spatterAmount;
        public float spawnDelay;
        public Sprite spatterSprite;
        public bool randomRotation = true;
        public float fadeDelay;
        public float fadeSpeed;
        public CanvasGroup canvasGroup;
        private bool implode = false;

        private void Start()
        {
            StartCoroutine(SpawnSpatterAndFadeOut());
        }

        private void Update()
        {
            if (implode)
                Destroy(transform.parent.gameObject);
        }

        private IEnumerator SpawnSpatterAndFadeOut()
        {
            int toSpawn = spatterAmount;
            Vector3 screenPos = GetComponent<RectTransform>().position;

            while (toSpawn > 0)
            {
                float posX = screenPos.x + Random.Range(-spatterRadius, spatterRadius);
                float posY = screenPos.y + Random.Range(-spatterRadius, spatterRadius);

                GameObject spatterObj = new GameObject("Spatter", new[]
                {
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image),
                });

                spatterObj.GetComponent<Image>().sprite = spatterSprite;
                RectTransform rectTransform = spatterObj.GetComponent<RectTransform>();
                rectTransform.SetParent(transform);
                rectTransform.position = new Vector3(posX, posY, 0);
                rectTransform.sizeDelta = new Vector2(spatterSize, spatterSize);
                if (randomRotation)
                    rectTransform.Rotate(Vector3.forward, Random.Range(0, 360));
                
                toSpawn--;
                yield return new WaitForSecondsRealtime(spawnDelay);
            }

            yield return new WaitForSecondsRealtime(fadeDelay);

            float alpha = 1.0f;
            while (alpha > 0f)
            {
                canvasGroup.alpha = alpha;
                alpha -= fadeSpeed;
                yield return new WaitForEndOfFrame();
            }
            
            canvasGroup.alpha = 0.0f;
            implode = true;
        }
    }
}
