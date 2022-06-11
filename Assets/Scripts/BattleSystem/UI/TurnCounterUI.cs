using System.Collections.Generic;
using Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class TurnCounterUI : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private List<TurnBlock> turnBlocks = new List<TurnBlock>();
        
        public void PopulateTurnQueueUI(EntityScriptable[] queue)
        {
            for (int i = 0; i < turnBlocks.Count; i++)
            {
                turnBlocks[i].playerPortrait.SetActive(false);
                turnBlocks[i].gummoPortrait.SetActive(false);
                turnBlocks[i].sophiePortrait.SetActive(false);
                //turnBlocks[i].sivePortrait.SetActive(false);
                
                turnBlocks[i].enemyTag.SetActive(false);
                
                
                string entityID = queue[i % queue.Length].entityId;

                if (entityID.Equals("_player"))
                    turnBlocks[i].playerPortrait.SetActive(true);
                else if (entityID.Equals("_gummo"))
                    turnBlocks[i].gummoPortrait.SetActive(true);
                else if (entityID.Equals("_sandra"))
                    turnBlocks[i].sophiePortrait.SetActive(true);
                // else if (entityID.Equals("_sive"))
                //     turnBlocks[i].sivePortrait.SetActive(true);
                else
                    turnBlocks[i].enemyTag.SetActive(true);
            }
        }
        
        [System.Serializable]
        private struct TurnBlock
        {
            public GameObject playerPortrait;
            public GameObject gummoPortrait;
            public GameObject sophiePortrait;
            public GameObject sivePortrait;
            
            public GameObject enemyTag;
        }
    }
}
