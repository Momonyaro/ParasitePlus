using System.Collections;
using Scriptables;
using UnityEngine;

namespace BATTLE
{
    public class BMGraphicalInterface : MonoBehaviour
    {
        public Transform currentEntitySlot;
        public Transform[] partySlots = new Transform[4];
        public CharStatusUI[] partyUI = new CharStatusUI[4];
        public Transform[] enemySlots = new Transform[9];

        [Space]

        public GameObject selectorPrefab;
        private GameObject selectorInstance;
        private BattleManager bManager;
        private int selectorIndex;
        private float aTimer;
        public float timeBetweenMove = 0.2f;
        public bool debugTargetAll = false;

        private const string RESOURCE_PATH = "Entities/";

        // We can return to the overview state if the player presses the "select" button or if all the entities
        //  are in either a 'dead' or 'idle' animator state.


        public void PopulateBoard(EntityScriptable active, EntityScriptable[] party, EntityScriptable[] enemies, bool hideDuplicates = true)
        {
            //Here we fetch the corresponding graphical entity to represent the data.
            //This should only be called once and then we can access the data with a 1 to 1 representation to the graphical elements.

            WipeOldSlot(currentEntitySlot);
            FetchAndPlaceResource(currentEntitySlot, active.entityId);

            for (int i = 0; i < party.Length; i++)
            {
                WipeOldSlot(partySlots[i]);
                if (party[i] == null || party[i].deadTrigger) continue;
                if (!party[i].throwawayId.Equals(active.throwawayId) || !hideDuplicates)
                {
                    FetchAndPlaceResource(partySlots[i], party[i].entityId);
                }
            }

            if (bManager.flags[0])
            {
                foreach (var slot in enemySlots) { WipeOldSlot(slot); }

                if (party[0] != null && !party[0].deadTrigger && !party[0].throwawayId.Equals(active.throwawayId) || !hideDuplicates)
                {
                    FetchAndPlaceResource(enemySlots[1], party[0].entityId);
                }
                if (party[1] != null && !party[1].deadTrigger && !party[1].throwawayId.Equals(active.throwawayId) || !hideDuplicates)
                {
                    FetchAndPlaceResource(enemySlots[2], party[1].entityId);
                }
                if (party[2] != null && !party[2].deadTrigger && !party[2].throwawayId.Equals(active.throwawayId) || !hideDuplicates)
                {
                    FetchAndPlaceResource(enemySlots[3], party[2].entityId);
                }
                if (party[3] != null && !party[3].deadTrigger && !party[3].throwawayId.Equals(active.throwawayId) || !hideDuplicates)
                {
                    FetchAndPlaceResource(enemySlots[5], party[3].entityId);
                }
            }
            else
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    WipeOldSlot(enemySlots[i]);
                    if (enemies[i] == null || enemies[i].deadTrigger) continue;
                    if (!enemies[i].throwawayId.Equals(active.throwawayId) || !hideDuplicates)
                    {
                        FetchAndPlaceResource(enemySlots[i], enemies[i].entityId);
                    }
                }
            }
        }

        private void Awake()
        {
            bManager = GetComponent<BattleManager>();
        }

        // This could be a bit wack... Perhaps investigate if a better way is possible.
        void Update()
        {
            aTimer -= Time.deltaTime;
        }

        //Here we fetch the GameObjects that correspond to the entityIds and place them at their correct board positions.
        public void FetchAndPlaceResource(Transform parent, string entityId)
        {
            GameObject resource;

            try { resource = Resources.Load<GameObject>(RESOURCE_PATH + entityId); }
            catch (System.Exception) { Debug.LogError("Could not load resource! " + RESOURCE_PATH + entityId); return; }

            resource = Resources.Load<GameObject>(RESOURCE_PATH + entityId);

            Instantiate(resource, parent);
        }

        //This just wipes all graphical objects from the board, keeping the pivots.
        private void WipeOldSlot(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        // A bit crude but this is responsible for moving the board cursor. This will be used to select a target for attacks.
        public void NavigateEntityGrid(Vector2 movementDir)
        {
            if (selectorInstance == null) return;
            if (aTimer > 0) return;

            bool xDead = false, yDead = false;

            if (Mathf.Abs(movementDir.x) < 0.15f) { movementDir.x = 0; xDead = true; } //Deadzone
            if (Mathf.Abs(movementDir.y) < 0.15f) { movementDir.y = 0; yDead = true; } //Deadzone

            if (xDead && yDead) return;

            if (Mathf.Abs(movementDir.x) >= Mathf.Abs(movementDir.y))   // Moving horizontally
            {
                if (movementDir.x > 0) // Moving to the [RIGHT]
                {
                    switch(selectorIndex) {
                        case 0: selectorIndex = 2; break;
                        case 2: selectorIndex = 5; break;
                        case 5: selectorIndex = 5; break;
                        case 7: selectorIndex = 5; break;
                        case 8: selectorIndex = 7; break;
                        default: selectorIndex++; break;
                    }
                }
                else
                {
                    switch (selectorIndex) {
                        case 0: selectorIndex = 1; break;
                        case 1: selectorIndex = 3; break;
                        case 3: selectorIndex = 3; break;
                        case 6: selectorIndex = 3; break;
                        case 8: selectorIndex = 6; break;
                        default: selectorIndex--;  break;
                    }
                }
            }
            else // Moving vertically
            {
                if (movementDir.y > 0) // Moving [UP]
                {
                    switch (selectorIndex)
                    {
                        case -1: selectorIndex = 0; break;
                        case 1: selectorIndex = 6; break;
                        case 2: selectorIndex = 7; break;
                        case 3: selectorIndex = 6; break;
                        case 6: selectorIndex = 8; break;
                        case 8: selectorIndex = 8; break;
                        case 7: selectorIndex = 8; break;
                        case 5: selectorIndex = 7; break;
                        default: selectorIndex += 4; break;
                    }
                }
                else
                {
                    switch (selectorIndex)
                    {
                        case 3: selectorIndex = 1; break;
                        case 1: selectorIndex = 0; break;
                        case 0: selectorIndex = -1; break;
                        case 2: selectorIndex = 0; break;
                        case 5: selectorIndex = 2; break;
                        case 6: selectorIndex = 1; break;
                        case 7: selectorIndex = 2; break;
                        default: selectorIndex -= 4; break;
                    }
                }
            }

            aTimer = timeBetweenMove;
            MoveSelectorToIndex();
        }

        //This method actually moves the cursor to the correct board position based on the selectorIndex
        private void MoveSelectorToIndex()
        {
            selectorIndex = Mathf.Clamp(selectorIndex, -1, 8);

            if (selectorIndex > -1) //Enemy matrix
            {
                selectorInstance.transform.SetParent(enemySlots[selectorIndex]);
                selectorInstance.transform.localPosition = Vector3.zero;
                //Since the selector is billboarded we don't care about rotation!
            }
            else //Center entity
            {
                selectorInstance.transform.SetParent(currentEntitySlot);
                selectorInstance.transform.localPosition = Vector3.zero;
                //Since the selector is billboarded we don't care about rotation!
            }
        }
    
        //Spawns the board cursor and allows for it to be moved.
        public void SpawnSelector()
        {
            if (selectorInstance != null) DestroySelector();
            selectorIndex = 4;  // This is to place it in the center
            selectorInstance = Instantiate(selectorPrefab, enemySlots[selectorIndex]);
            StartCoroutine(SpriteColorPan());
        }

        //De-spawns the board cursor and disallows for it's position to be changed
        public void DestroySelector()
        {
            Destroy(selectorInstance);
            selectorInstance = null;
        }

        private IEnumerator SpriteColorPan()
        {
            SpriteRenderer sprRenderer = selectorInstance.transform.parent.GetChild(0).GetComponent<SpriteRenderer>();
            float colorSinValue;
            float panSpeed = 3f;
            bool targetAll;

            while (true)
            {
                targetAll = bManager.flags[5];
                if (!bManager.flags[2]) { break; };
                if (selectorInstance.transform.parent.childCount > 0 && selectorInstance.transform.parent.GetChild(0).GetComponent<SpriteRenderer>() != null)
                    sprRenderer = selectorInstance.transform.parent.GetChild(0).GetComponent<SpriteRenderer>();
                else
                    sprRenderer = null;

                colorSinValue = Mathf.Clamp(Mathf.Sin(Time.time * panSpeed), 0.3f, 0.95f);

                if (currentEntitySlot.childCount > 0) currentEntitySlot.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;

                for (int i = 0; i < enemySlots.Length; i++)
                {
                    if (enemySlots[i].childCount > 0 && enemySlots[i].GetChild(0).GetComponent<SpriteRenderer>() != null)
                    {
                        if (targetAll)  enemySlots[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(colorSinValue, colorSinValue, 1);
                        else            enemySlots[i].GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }

                for (int i = 0; i < partySlots.Length; i++)
                {
                    if (partySlots[i].childCount > 0 && partySlots[i].GetChild(0).GetComponent<SpriteRenderer>() != null)
                    {
                        if (targetAll)  partySlots[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(colorSinValue, colorSinValue, 1);
                        else            partySlots[i].GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }

                if (sprRenderer != null)
                    sprRenderer.color = new Color(colorSinValue, colorSinValue, 1);

                yield return new WaitForFixedUpdate();
            }

            if (sprRenderer != null)
                sprRenderer.color = new Color(1, 1, 1);

            yield break;
        }

        public bool CheckIfValidTarget()
        {
            if (bManager.flags[7]) return false;
            if (bManager.flags[5]) return true;
            
            if (selectorIndex > -1)
            {
                if (enemySlots[selectorIndex].childCount > 0 
                    && enemySlots[selectorIndex].GetChild(0).GetComponent<SpriteRenderer>() != null)
                    return true;
            }
            return false;
        }

        //A simple getter for the board index.
        public int GetSelectorPosition()
        {
            return selectorIndex;
        }

        public void SetSelectorPosition(int newIndex)
        {
            selectorIndex = newIndex;
        }

        //A simple state checker to see if the selector is spawned or not.
        public bool IsSelectorNull()
        {
            return (selectorInstance == null);
        }
    }
}
