using System.Collections;
using System.Collections.Generic;
using Scriptables;
using UnityEngine;

namespace BATTLE
{
    public class BMTurnClock
    {
        public List<TurnElement> turnQueue; //This is not a queue to make it easier to slot entities in if they should be before the last entry.
        public int currentTurn = 0;         //This value keeps track of turns processed and uses this to affect entity turn points.

        //This creates the base queue for all entities that are on the board at battle start.
        public void Init(EntityScriptable[] party, EntityScriptable[] enemies)
        {
            turnQueue = new List<TurnElement>();

            for (int i = 0; i < party.Length; i++)
            {
                if (party[i] == null) { continue; }

                //Points will be ran through an algorithm when entering the queue;
                AddItemToTurnQueue(new TurnElement(party[i].entityName, party[i].throwawayId, party[i].GetEntityStats()[(int)EntityScriptable.STAT.AGILITY], false));  
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null) { continue; }

                //Points will be ran through an algorithm when entering the queue;
                AddItemToTurnQueue(new TurnElement(enemies[i].entityName, enemies[i].throwawayId, enemies[i].GetEntityStats()[(int)EntityScriptable.STAT.AGILITY], true));  
            }
        }

        public void Tick()
        {
            currentTurn++;
            for (int i = 0; i < turnQueue.Count; i++)
            {
                TurnElement q = turnQueue[i];
                q.points += Mathf.RoundToInt(q.points / 2.0f);
                turnQueue[i] = q;
            }
        }

        //This returns the entity that is at the front of the queue and removes it from the queue.
        public TurnElement ReturnNextEntity()
        {
            TurnElement nextElement = turnQueue[0];
            turnQueue.RemoveAt(0);

            return nextElement;
        }

        //DEBUG, This just tells us the id and points of all entities in the queue, ordered from front to back.
        public string TestTickQueue(int state)
        {
            string debugText = $"currentTurn: {currentTurn}\n";
            debugText += $"currentBMState: {state}\n";
            for (int i = 0; i < turnQueue.Count; i++)
            {
                debugText += $"[ TURN ELEMENT <{i}> ] || entityName: [ {turnQueue[i].entityName} ] | entityID: [ {turnQueue[i].entityId} ] | points: [ {turnQueue[i].points} ] | enemyFlag: [ {turnQueue[i].enemyFlag} ] |\n";
            }
            return debugText;
        }

        //This places an entity into the queue based on its points.
        public void AddItemToTurnQueue(TurnElement newElement)
        {
            for (int i = 0; i < turnQueue.Count; i++)
            {
                if (turnQueue[i].points < newElement.points)
                {
                    turnQueue.Insert(i, newElement);
                    return;
                }
            }
            turnQueue.Add(newElement);
        }

        //Removes an entity from the queue by matching its entityId with an entity's throwawayId.
        public void RemoveItemFromTurnQueue(int entityId)
        {
            for (int i = 0; i < turnQueue.Count; i++)
            {
                if (turnQueue[i].entityId == entityId)
                {
                    turnQueue.RemoveAt(i);  // Success! We found the turnEntity and can now delete it safely!
                    return;
                }
            }
            // Failure! Dropping down means we did not find an entry which could mean that we call a deleted entity!
        }

    }

    public struct TurnElement
    {
        public string entityName;
        public int entityId;
        public int points;
        public bool enemyFlag;

        public TurnElement(string entityName, int entityId, int points, bool enemyFlag)
        {
            this.entityName = entityName;
            this.entityId = entityId;
            this.points = points;
            this.enemyFlag = enemyFlag;
        }
    }
}