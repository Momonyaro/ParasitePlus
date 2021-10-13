using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace BattleSystem
{
    [System.Serializable]
    public class BattleSystemTurnOrder
    {
        //This component is responsible for creating the turn queue and giving each entity a unique id
        // during the battle.
        
        // So how is the turn queue actually gonna work?
        // basically, we give each entity a score each turn based on their agility and a small random factor.
        // this will accumulate each turn until they execute their turn, after which their score is reset.

        public List<TurnItem> turnQueue = new List<TurnItem>();

        public int endOfTurnPoints = 100;
        
        private const int IDMax = 0xff; // 0xff = 255;
        private const int IDMin = 0x00;


        private List<int> usedIDs = new List<int>();
        
        public void Init(ref EntityScriptable[] party, ref EntityScriptable[] enemies)
        {
            usedIDs.Clear();

            for (int i = 0; i < party.Length; i++)
            {
                if (party[i] != null)
                {
                    party[i].throwawayId = GetUniqueID();
                    
                    turnQueue.Add(new TurnItem(
                        party[i].entityName, 
                        party[i].throwawayId, 
                        party[i].entitySpeed, 
                        false, //False since we're reading the party's data
                        0)); 
                    
                    //Debug.Log($"[TurnQueue] : Gave entity {i} with name: {party[i].entityName}, ID: [{party[i].throwawayId}]");
                }
            }
            
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].throwawayId = GetUniqueID();
                    
                    turnQueue.Add(new TurnItem(
                        enemies[i].entityName, 
                        enemies[i].throwawayId, 
                        enemies[i].entitySpeed, 
                        true, //True since we're reading the enemy data
                        0)); 
                    
                    //Debug.Log($"[TurnQueue] : Gave entity {i} with name: {enemies[i].entityName}, ID: [{enemies[i].throwawayId}]");
                }
            }

            SortAndParseTurnQueue();
            
            PrintCurrentTurnQueue();
        }

        public void ModifyEntityTurnScore(int entityId, int difference)
        {
            for (int i = 0; i < turnQueue.Count; i++)
            {
                if (turnQueue[i].entityId == entityId)
                {
                    TurnItem temp = turnQueue[i];
                    temp.score = Mathf.Max(0, turnQueue[i].score + difference); //Limit to 0 so that there is no chance of negatives.
                    turnQueue[i] = temp;
                }
            }
        }
        
        private void SortAndParseTurnQueue() // Mostly used for initialization, use a insertion method for mid-combat changes?
        {
            List<TurnItem> toSort = new List<TurnItem>(turnQueue);
            List<TurnItem> sorted = new List<TurnItem>();

            while (toSort.Count > 0)
            {
                int highest = 0;
                int index = 0;
                for (int i = 0; i < toSort.Count; i++)
                {
                    if (toSort[i].score >= highest) // >= could cause problems with entities with equal scores but for now it's fine
                    {
                        highest = toSort[i].score;
                        index = i;
                    }
                }
                sorted.Add(toSort[index]);
                toSort.RemoveAt(index);
            }

            turnQueue = sorted;
        }

        public TurnItem GetFirstInLine()
        {
            if (turnQueue.Count == 0) return new TurnItem("QUEUE EMPTY", -512, 0, false, -1);
            
            return turnQueue[0];
        }

        public void AddEndOfTurnPointsToQueue()
        {
            for (int i = 0; i < turnQueue.Count; i++)
            {
                TurnItem item = turnQueue[i];
                item.score += endOfTurnPoints;
                turnQueue[i] = item;
            }
        }

        public void CycleTurnQueue()
        {
            TurnItem last = turnQueue[0];

            last.turnsTaken++;

            //Reset it's score, I'm doing it by just re-creating the item.
            last = new TurnItem(last.entityName, last.entityId, last.entityAgility, last.enemyTag, last.turnsTaken);
            
            turnQueue.RemoveAt(0);
            turnQueue.Add(last);
            
            SortAndParseTurnQueue();
            PrintCurrentTurnQueue();
        }

        private int GetUniqueID()
        {
            int attempts = 100;
            while (attempts > 0)
            {
                int random = Random.Range(IDMin, IDMax);
                if (!usedIDs.Contains(random)) //If we have not already used this ID, hand it out
                {
                    usedIDs.Add(random);
                    return random;
                }

                attempts--;
            }

            int failsafe = Random.Range(-IDMax, IDMin);
            usedIDs.Add(failsafe);
            return failsafe; // As a safety measure we can return a negative number if we failed to get a positive one.
        }

        private void PrintCurrentTurnQueue() // Mostly for debugging, could prove useful
        {
            string debugMsg = "";
            for (int i = 0; i < turnQueue.Count; i++)
            {
                debugMsg += $"[TurnQueue] : [{i}], ID: {turnQueue[i].entityId}, Name: {turnQueue[i].entityName}, Score: {turnQueue[i].score}, Agility: {turnQueue[i].entityAgility}, Enemy: {turnQueue[i].enemyTag}\n";
            }
            Debug.Log(debugMsg);
        }
    }

    [System.Serializable]
    public struct TurnItem
    {
        private const int RandomPointOffset = 5; // +- 5 points
        
        public string entityName;
        public int entityId;
        public int entityAgility;
        public int score;
        public int turnsTaken;
        public bool enemyTag;

        public TurnItem(string entityName, int entityId, int entityAgility, bool enemyTag, int turnsTaken)
        {
            this.entityName = entityName;
            this.entityId = entityId;
            this.entityAgility = entityAgility;
            this.score = Mathf.Max(0, entityAgility + Random.Range(-RandomPointOffset, RandomPointOffset));
            this.enemyTag = enemyTag;
            this.turnsTaken = turnsTaken;
        }
    }
}
