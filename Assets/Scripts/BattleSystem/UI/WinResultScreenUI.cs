using System;
using System.Collections;
using System.Collections.Generic;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace BattleSystem.UI
{
    public class WinResultScreenUI : MonoBehaviour
    {
        public float barFillSpeed = 0.2f;
        public float remnantXpFillTime = 0.5f;
        public float waitTime = 1.2f;
        public bool finished = false;
        public LevelUpContainer[] levelUpContainers = new LevelUpContainer[3];

        public Sprite playerSpr;
        public Sprite gummoSpr;
        public Sprite sandraSpr;
        public Sprite siveSpr;

        private void OnValidate()
        {
            for (int i = 0; i < levelUpContainers.Length; i++)
            {
                levelUpContainers[i].title = "Level Up Data [" + i + "]";
            }
        }

        public void DisplayLevelUp(EntityScriptable[] party, int xpToAdd, float randomRange)
        {
            finished = false;
            int[] xpRewards = new int[party.Length];

            for (int i = 0; i < party.Length; i++)
            {
                if (party[i] != null)
                {
                    Sprite selected = playerSpr;

                    switch (party[i].entityId)
                    {
                        case "_gummo":  selected = gummoSpr;  break;
                        case "_sandra": selected = sandraSpr; break;
                        case "_sive":   selected = siveSpr;   break;
                    }

                    levelUpContainers[i].portrait.sprite = selected;
                }
            }
            
            for (int i = 0; i < levelUpContainers.Length; i++)
            {
                if (party[i] == null)
                {
                    levelUpContainers[i].parent.gameObject.SetActive(false);
                    continue;
                }

                int xpReward = xpToAdd + Mathf.FloorToInt(Random.Range(-xpToAdd * randomRange, xpToAdd * randomRange));
                if (party[i].deadTrigger)
                    xpReward = 0; //No exp if ye' dead

                xpRewards[i] = xpReward;
            }

            StartCoroutine(LevelUpEnumerator(party, xpRewards));
        }

        private IEnumerator LevelUpEnumerator(EntityScriptable[] party, int[] xpToAdd)
        {
            
            int[] levelDifference = new int[party.Length];
            
            for (int i = 0; i < levelDifference.Length; i++)
            {
                if  (party[i] == null) continue;
                int originalLevel = party[i].entityLevel;
                int originalXp = party[i].entityXp;
                int originalXpThreshold = party[i].entityXpThreshold;
                levelDifference[i] = originalLevel;
                party[i].AddXpToEntity(xpToAdd[i]);

                for (int j = 0; j < party[i].abilityScriptables.Length; j++)
                {
                    if  (party[i] == null) continue;
                    party[i].abilityScriptables[j].ResetValues();
                }
                
                levelDifference[i] = party[i].entityLevel - levelDifference[i]; // Should yield the amount of levels gained this level-up.
                
                Debug.Log($"start: {originalLevel}, end: {party[i].entityLevel}, diff: {levelDifference[i]}");
                
                //Debug.Assert(i != 3); // If we're reading a fourth party-member something is wrong.
                
                //Reset xp-bar
                levelUpContainers[i].xpBar.fillAmount = (float) originalXp / originalXpThreshold;
                levelUpContainers[i].levelPlus.gameObject.SetActive(false);
                int croppedLvl = party[i].entityLevel - levelDifference[i];
                levelUpContainers[i].levelNumText.text = croppedLvl.ToString();
            }
            
            yield return new WaitForSeconds(waitTime);

            // For each level that was gained, we fill the bar to the brim.
            bool lvlDiffNotZero = true;
            while (lvlDiffNotZero)
            {
                for (int i = 0; i < levelDifference.Length; i++) // Check if there is a level still to use
                {
                    if (levelDifference[i] == 0)
                        continue;

                    float currentFill = levelUpContainers[i].xpBar.fillAmount;
                    currentFill = Mathf.Clamp(currentFill + (barFillSpeed * Time.deltaTime), 0, 1);
                    levelUpContainers[i].xpBar.fillAmount = currentFill;

                    if (currentFill >= 0.99f)
                    {
                        levelUpContainers[i].xpBar.fillAmount = 0;
                        levelUpContainers[i].levelPlus.gameObject.SetActive(true);
                        
                        levelDifference[i]--;
                        
                        int croppedLvl = party[i].entityLevel - levelDifference[i];
                        levelUpContainers[i].levelNumText.text = croppedLvl.ToString();
                    }
                }
                
                //Break out of the loop if all lvlDiff values read zero.
                int diffAllZero = levelDifference.Length;
                for (int i = 0; i < levelDifference.Length; i++)
                {
                    if (levelDifference[i] == 0)
                        diffAllZero--;
                }

                if (diffAllZero == 0)
                    lvlDiffNotZero = false;

                yield return new WaitForEndOfFrame();
            }
            
            //Fill the bar to the current level of xp/xpThreshold
            float timePassed = 0;
            while (timePassed < remnantXpFillTime)
            {
                for (int i = 0; i < levelUpContainers.Length; i++)
                {
                    if (party[i] == null)
                        continue;
                    
                    float multiplePercentage = timePassed / remnantXpFillTime;
                    float xpPercentage = (float) party[i].entityXp / party[i].entityXpThreshold;
                    levelUpContainers[i].xpBar.fillAmount = xpPercentage * multiplePercentage;
                }
                
                timePassed += Time.deltaTime;
                
                yield return new WaitForEndOfFrame();
            }

            finished = true;
            yield break;
        }
    }

    [Serializable]
    public class LevelUpContainer
    {
        [HideInInspector] public string title = "Level Up Data";
        public Transform parent;
        public Image xpBar;
        public Image portrait;
        public TextMeshProUGUI levelNumText;
        public GameObject levelPlus;

        public AnimationCurve barAccelCurve = new AnimationCurve();
    }
}
