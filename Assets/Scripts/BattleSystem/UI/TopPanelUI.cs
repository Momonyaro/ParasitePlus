using System;
using System.Collections;
using System.Collections.Generic;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class TopPanelUI : MonoBehaviour
    {
        // Here we need to store references to the party's cards to set health and action points
        // as well as to choose to extend them or not.
        
        public Animator topAnimator;
        public Image backgroundPanel;
        public AnimationCurve shakeLerpCurve = new AnimationCurve();
        public float backgroundFadeSpeed = 10f;
        public bool finishedFoldingOut = false;
        [SerializeField] private UICard[] partyCards = new UICard[3];

        public void PopulatePartyCards(EntityScriptable[] party, int currentActiveId)
        {
            for (int i = 0; i < partyCards.Length; i++)
            {
                if (party[i] == null)
                {
                    partyCards[i].parent.SetActive(false); 
                    continue;
                }

                string entityID = party[i].entityId;
                
                partyCards[i].playerPortrait.SetActive(entityID.Equals("_player"));
                partyCards[i].gummoPortrait.SetActive(entityID.Equals("_gummo"));
                //partyCards[i].sophiePortrait.SetActive(entityID.Equals("_sophie"));
                //partyCards[i].sivePortrait.SetActive(entityID.Equals("_sive"));

                Vector2Int hp = party[i].GetEntityHP();
                Vector2Int ap = party[i].GetEntityAP();
                
                partyCards[i].hpText.text = hp.x.ToString();
                partyCards[i].apText.text = ap.x.ToString();

                partyCards[i].hpBarFill.fillAmount = (float)hp.x / hp.y;
                partyCards[i].apBarFill.fillAmount = (float)ap.x / ap.y;
                
                partyCards[i].activeTag.SetActive(currentActiveId == party[i].throwawayId);
            }
        }

        public void SetFoldoutState(bool visibility)
        {
            topAnimator.SetBool("ShowParty", visibility);
            StartCoroutine(FadeBackground(visibility));
        }

        public void ShakePlayerCard(int cardIndex)
        {
            StartCoroutine(ShakeCardPortrait(cardIndex, 2.0f, 300f));
        }

        private IEnumerator FadeBackground(bool visibility)
        {
            float finalAlpha = (visibility) ? .8f : .0f;
            
            Color finalColor = new Color(0, 0, 0, finalAlpha);
            Color currentColor = backgroundPanel.color;

            while (Mathf.Abs(finalColor.a - currentColor.a) >= 0.005f)
            {
                backgroundPanel.color = Color.Lerp(currentColor, finalColor, backgroundFadeSpeed * Time.deltaTime);
                currentColor = backgroundPanel.color;
                
                yield return new WaitForEndOfFrame();
            }

            backgroundPanel.color = finalColor;

            finishedFoldingOut = visibility;
            
            yield break;
        }

        public Vector2 GetCardPosition(int index, out bool cardExists)
        {
            cardExists = true;
            if (!partyCards[index].parent.activeInHierarchy)
            {
                cardExists = false;
            }

            return partyCards[index].parent.GetComponent<RectTransform>().position;
        }

        public int MoveToNextCard(int currentIndex, int movementDelta)
        {
            int searchIndex = currentIndex + movementDelta;
            while (true)
            {
                if (searchIndex >= partyCards.Length) searchIndex = searchIndex % partyCards.Length;
                if (searchIndex < 0) searchIndex = partyCards.Length - 1;
                if (partyCards[searchIndex].parent.activeInHierarchy)
                {
                    return searchIndex;
                }

                searchIndex += movementDelta; // move it another step in the direction we want if we did not find anything.
            }
        }

        private IEnumerator ShakeCardPortrait(int cardIndex, float timeScale, float magnitude)
        {
            UICard current = partyCards[cardIndex];
            RectTransform portraitTransform = current.portraitParent;
            Vector2 origo = portraitTransform.anchoredPosition;

            float timePassed = 0;
            float lerpCurveLastX = shakeLerpCurve.keys[shakeLerpCurve.length - 1].time;
            portraitTransform.anchoredPosition = origo;

            while (timePassed < lerpCurveLastX)
            {
                Vector2 shakeVec = new Vector2(shakeLerpCurve.Evaluate(timePassed), 0);

                var currentCamPos = portraitTransform.anchoredPosition;
                Vector3 lerped = Vector2.Lerp(currentCamPos, origo + (shakeVec * magnitude), 0.5f);

                portraitTransform.anchoredPosition = lerped;

                timePassed += Time.deltaTime * 2.0f;
                timePassed = Mathf.Clamp(timePassed, 0, lerpCurveLastX);
                yield return new WaitForEndOfFrame();
            }

            portraitTransform.anchoredPosition = origo;

            yield break;
        }
        
        
        [System.Serializable]
        private struct UICard
        {
            public GameObject parent;
            public RectTransform portraitParent;
            
            public Image hpBarFill;
            public Image apBarFill;

            public TextMeshProUGUI hpText;
            public TextMeshProUGUI apText;

            public GameObject playerPortrait;
            public GameObject gummoPortrait;
            public GameObject sophiePortrait;
            public GameObject sivePortrait;
            
            public GameObject activeTag;
        }
    }
}
