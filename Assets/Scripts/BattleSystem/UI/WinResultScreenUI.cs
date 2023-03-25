﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public TextMeshProUGUI cashRewardText;
        public TextMeshProUGUI walletText;

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

        public void UpdateMoney(int reward, int wallet)
        {
            string rewards = $"{reward} SEK";
            string walletString = $"Total: {wallet} SEK";

            cashRewardText.text = rewards;
            walletText.text = walletString;
        }

        public void DisplayLevelUp(EntityScriptable[] party, int[] xpToAdd, float randomRange)
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
                    party[i].CalculateNewXpThreshold(); // to avoid funny divide by 0

                    int xpReward = xpToAdd[i] + Mathf.FloorToInt(Random.Range(-xpToAdd[i] * randomRange, xpToAdd[i] * randomRange));
                    if (party[i].deadTrigger)
                        xpReward = 0; //No exp if ye' dead

                    xpRewards[i] = xpReward;
                }
            }
            
            for (int i = 0; i < levelUpContainers.Length; i++)
            {
                if (party[i] == null)
                {
                    levelUpContainers[i].parent.gameObject.SetActive(false);
                    continue;
                }
            }

            StartCoroutine(LevelUpEnumerator(party.Where(e => e != null).ToArray(), xpRewards));
        }

        private IEnumerator LevelUpEnumerator(EntityScriptable[] party, int[] xpToAdd)
        {
            
            int[] levelDifference = new int[party.Length];
            
            for (int i = 0; i < levelDifference.Length; i++)
            {
                int originalLevel = party[i].entityLevel;
                int originalXp = party[i].entityXp;
                int originalXpThreshold = party[i].entityXpThreshold;
                levelDifference[i] = originalLevel;
                party[i].AddXpToEntity(xpToAdd[i]);
                
                levelDifference[i] = party[i].entityLevel - levelDifference[i]; // Should yield the amount of levels gained this level-up.
                
                Debug.Log($"name: {party[i].entityName} start: {originalLevel}, end: {party[i].entityLevel}, diff: {levelDifference[i]}");
                
                //Debug.Assert(i != 3); // If we're reading a fourth party-member something is wrong.
                
                //Reset xp-bar
                levelUpContainers[i].xpBar.fillAmount = (float)originalXp / originalXpThreshold;
                if (levelDifference[i] > 0) levelUpContainers[i].xpBar.fillAmount = 0;

                levelUpContainers[i].levelPlus.gameObject.SetActive(false);
                levelUpContainers[i].levelNumText.text = originalLevel.ToString();
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

                    float lastFill = levelUpContainers[i].xpBar.fillAmount;
                    float currentFill = Mathf.Clamp(lastFill + (barFillSpeed * Time.deltaTime), 0, 1);
                    Debug.Log($"lFill: {lastFill}, cFill: {currentFill}, xpBarFill: {levelUpContainers[i].xpBar.fillAmount}");
                    levelUpContainers[i].xpBar.fillAmount = currentFill;

                    if (currentFill >= 0.95f)
                    {
                        levelUpContainers[i].xpBar.fillAmount = 0;
                        levelUpContainers[i].levelPlus.gameObject.SetActive(true);
                        
                        levelDifference[i]--;
                        
                        int croppedLvl = party[i].entityLevel - levelDifference[i];
                        Debug.Log(croppedLvl + ", diff: " + levelDifference[i]);
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

                Debug.Log(String.Join(", ", levelDifference));

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
                    if (i >= party.Length)
                        continue;
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
