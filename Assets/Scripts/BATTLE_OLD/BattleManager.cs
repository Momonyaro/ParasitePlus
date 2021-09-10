using System.Collections;
using System.Collections.Generic;
using CORE;
using Items;
using Scriptables;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BATTLE
{
    public class BattleManager : MonoBehaviour
    {
        public BMTurnClock turnComponent = new BMTurnClock();
        public BmBattleLogic battleLogic = new BmBattleLogic();
        public BMGraphicalInterface graphicalInterface;
        public BattleNavigator battleNavigator;
        public Text debug;

        [Space] 
        public GameObject winMenu;
        public GameObject lossMenu;
        // Do we want to break out turn logic into another module?

        // When going from the action state to the overview state we can call Populate board or something similar to reread the current board data
        //  and place the correct entities again.

        public bool[] flags = new bool[]
        {
            false,  // 0 = Enemy Active
            false,  // 1 = Action Phase
            false,  // 2 = Targeting
            false,  // 3 = Move Selected
            false,  // 4 = Dead Air
            false,  // 5 = Target All
            false,  // 6 = Timer Running
            false,  // 7 = refresh button
            false,  // 8 = reverted state
            false,  // 9 = submit button state
        };

        public EntityScriptable[] partyField = new EntityScriptable[4]; // When all party members are present, this should be full.
        public EntityScriptable[] enemyField = new EntityScriptable[9]; // The spots for the enemies will be ordered 0-9 to match this array.
        public List<Item> partyInventory = new List<Item>();
        public Vector2 lastPlayerPos = Vector2.zero;
        public int partyWallet = 0;
        public int lastDungeonIndex = 0;
        private EntityScriptable active = null;
        private List<int> idsInUse = new List<int>();
        public int battleState = 3;
        private int lastState = -1;
        private AbilityScriptable abilityToUse;

        private const int ID_LOWER_LIMIT = 0;
        private const int ID_UPPER_LIMIT = 25565;
        private int destinationScene;

        private void Start()
        {
            //Read the slim to fetch the current status of the party and the enemies for the encounter!
            LoadSlim();
            Init();
            turnComponent.Init( partyField, enemyField );
            turnComponent.TestTickQueue(battleState);

            ParseTurnState();
        }

        void Update()
        {
            //Inelegant but it works for now & uses the flags efficiently to toggle it's state.
            if (flags[7] && flags[9])  {  flags[7] = false; }
            flags[9] = false; 
            
            if (flags[2] && graphicalInterface.IsSelectorNull()) { graphicalInterface.SpawnSelector(); }
            if (!flags[2] && !graphicalInterface.IsSelectorNull()) { graphicalInterface.DestroySelector(); }
        }

        void LoadSlim()
        { 
            // SlimComponent.Instance.ReadVolatileSlim(out int slimSceneIndex, out EntityScriptable[] slimParty, out EntityScriptable[] slimEnemies, out List<Item> inventory, out Vector2 playerPos, out int money, out int dungeonIndex);
            // if (slimParty.Length != 4) //Erroneous data, party should always be 4.
            // {
            //     //Don't replace the data, it's garbo!
            // }
            // else
            // {
            //     destinationScene = slimSceneIndex;
            //     partyField = slimParty;
            //     enemyField = slimEnemies;
            //     partyInventory = inventory;
            //     partyWallet = money;
            //     lastPlayerPos = playerPos;
            //     lastDungeonIndex = dungeonIndex;
            // }
        }

        public void ReturnToPreviousScene()
        {
            //Create slim here before we load the scene.
            //SlimComponent.Instance.PopulateAndSendSlim(destinationScene, partyField, enemyField, partyInventory, lastPlayerPos, partyWallet, lastDungeonIndex);
            //Perhaps trigger some script that plays a transition between the scenes using dontDestroyOnLoad?
            Debug.Log("Loading scene: "+ destinationScene);
            SceneManager.LoadScene(destinationScene);
        }
        
        void Init()
        {
            idsInUse = new List<int>();
            for (int i = 0; i < partyField.Length; i++)
            {
                if (partyField[i] == null) continue;
                EntityScriptable entityCopy = partyField[i].Copy();

                entityCopy.throwawayId = AssignUnusedID();
                entityCopy.defaultAttack = partyField[i].defaultAttack;

                idsInUse.Add(entityCopy.throwawayId);
                partyField[i] = entityCopy;
            }

            for (int i = 0; i < enemyField.Length; i++)
            {
                if (enemyField[i] == null) continue;
                EntityScriptable entityCopy = enemyField[i].Copy();

                entityCopy.throwawayId = AssignUnusedID();
                entityCopy.defaultAttack = enemyField[i].defaultAttack;

                idsInUse.Add(entityCopy.throwawayId);
                enemyField[i] = entityCopy;
            }
        }

        //This method makes sure every entity has a different id.
        private int AssignUnusedID()
        {
            while (true)
            {
                var q = Random.Range(ID_LOWER_LIMIT, ID_UPPER_LIMIT);
                if (!idsInUse.Contains(q))
                    return q;
            }
        }

        private EntityScriptable ParseTurnData(TurnElement turnElement)
        {
            foreach (var t in partyField)
            {
                if (t != null && t.throwawayId.Equals(turnElement.entityId))
                {
                    return t;
                }
            }

            foreach (var t in enemyField)
            {
                if (t != null && t.throwawayId.Equals(turnElement.entityId))
                {
                    return t;
                }
            }

            return partyField[0];   //Fallback case!
        }

        public void RevertTurnState()
        {
            Debug.Log("Last State: " + lastState);
            if (lastState == 1) //Targeting state
            {
                battleState = lastState - 1;
                flags[8] = true;
                ParseTurnState();
            }
        }
        
        public void ParseTurnState()
        {
            int temp = battleState;
            switch (battleState)
            {
                case -1:
                    EntryState();
                    battleState++;
                    break;
                case 0:
                    if (lastState != 0) GoToActionState();
                    break;
                case 1:
                    ActionTargetState();
                    battleState++;
                    break;
                case 2:
                    if (graphicalInterface.CheckIfValidTarget()) { ActionExecState(); battleState++; }
                    else { battleState = 1; ParseTurnState(); }
                    break;
                case 3:
                    CleanBoardState();
                    
                    battleState = CheckWinOrLoss();
                    break;
                case 4:
                    WinState();
                    break;
                case 5:
                    LossState();
                    break;
            }

            lastState = temp;

            if (active == null) return;
            
            for (int i = 0; i < graphicalInterface.partyUI.Length; i++)
            {
                bool isActive = (active.throwawayId == partyField[i].throwawayId);
                var hpRatio = partyField[i].GetEntityHP();
                var mpRatio = partyField[i].GetEntityAP();
                graphicalInterface.partyUI[i].DrawUIElement(isActive, (float)hpRatio.x/hpRatio.y, (float)mpRatio.x/mpRatio.y);
            }
            
            flags[8] = false;
        }

        private void EntryState()
        {
            flags[1] = true;
            graphicalInterface.PopulateBoard(partyField[0], partyField, enemyField, false);
            battleNavigator.SetOverviewView();

            StartCoroutine(RunTimer(2f));
        }

        private int CheckWinOrLoss()
        {
            bool winCheck = true;
            foreach (var e in enemyField)
            {
                if (e == null) continue;
                if (!e.deadTrigger)
                {
                    winCheck = false;
                }
            }
            
            bool lossCheck = true;
            foreach (var e in partyField)
            {
                if (!e.deadTrigger)
                {
                    lossCheck = false;
                }
            }

            if (lossCheck)
            {
                //Return value to start the loss state so that we can exit the battle.
                return 5;
            }

            if (winCheck)
            {
                //Return value to start the win state so that we can exit the battle.
                return 4;
            }

            return 0;
        }

        private void WinState()
        {
            winMenu.SetActive(true);
        }
        
        private void LossState()
        {
            lossMenu.SetActive(true);
        }

        private void GoToActionState()   //State 0
        {
            // Here we select a new active entity, switch to action view and populate board.
            if (!flags[8])
            {
                TurnElement ts = turnComponent.ReturnNextEntity();
                flags[0] = ts.enemyFlag;
                active = ParseTurnData(ts);
            }
            flags[2] = false;
            flags[4] = false; //Dead air to disable ui buttons
            if (!flags[0]) { FindObjectOfType<BattleUI>().OnUiEnable(); }
            graphicalInterface.PopulateBoard(active, partyField, enemyField);
            battleNavigator.SetActionView();
            debug.text = turnComponent.TestTickQueue(battleState);

            if (flags[0]) StartCoroutine(BasicEnemyAttackCommand());
        }

        private void ActionTargetState()   // State 1
        {
            //We get here and wait for a target to be selected by checking flags[3] and after this has been done we move to the execstate
            //Perhaps we just enable targeting and if it's confirmed we just call the next state through that command?
            if (flags[0])
            {
                StartCoroutine(BasicEnemyAttackCommand());
            }
            else
            {
                flags[2] = true;
                flags[4] = true; //Dead air to disable ui buttons
            }
        }

        private void ActionExecState()   // State 2    Here we play the animations and call for damage calculations.
        {
            flags[2] = false;
            StartCoroutine(ExecuteAbility());
            StartCoroutine(RunTimer(1.2f));
        }

        private IEnumerator ExecuteAbility()
        {
            flags[6] = true;
            yield return new WaitForSeconds(1.2f);
            if (!abilityToUse.Execute(active, (flags[0]) ? partyField : enemyField, graphicalInterface.enemySlots, 
                graphicalInterface.GetSelectorPosition(), this))
            {
                battleState = 0;
                ParseTurnState();
            }
            flags[6] = false;
            yield break;
        }

        private IEnumerator RunTimer(float time)
        {
            while (flags[6]) { yield return null; /* Do nothing */ }
            flags[6] = true;
            yield return new WaitForSeconds(time);
            flags[6] = false;
            ParseTurnState();
            yield break;
        }

        public void ShakeCam()
        {
            StartCoroutine(battleNavigator.CameraShake(0.15f, 0.03f, -0.03f));
        }

        public AbilityScriptable[] GetActiveEntityAbilities()
        {
            return active.GetEntityAbilities();
        }

        private void CleanBoardState()   // State 3
        {
            // Here we populate the board again, switch view to overview and insert the active entity back into the turn queue.
            // We would want to wait until moves have been executed, skipped or animations have been completed before arriving here.
            turnComponent.Tick();
            turnComponent.AddItemToTurnQueue(new TurnElement(active.entityName, active.throwawayId, active.GetEntityStats()[(int)EntityScriptable.STAT.AGILITY], flags[0]));
            flags[0] = false;
            flags[4] = false;

            graphicalInterface.PopulateBoard(active, partyField, enemyField, false);
            battleNavigator.SetOverviewView();
            debug.text = turnComponent.TestTickQueue(battleState);
            flags[4] = false;   // This reset is so that we can reliably wait for dead air to occur again

            StartCoroutine(RunTimer(1.2f));
        }
        
        

        public void DefaultAttackCommand()
        {
            abilityToUse = active.defaultAttack;
            flags[5] = abilityToUse.targetAll;
            flags[7] = true;
            
            battleState = 1;
            ParseTurnState();
        }

        public void ParseSkillAttackCommand(string abilityId)
        {
            AbilityScriptable[] abilities = active.GetEntityAbilities();
            
            abilityToUse = active.defaultAttack;
            
            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].abilityId.Equals(abilityId))
                    abilityToUse = abilities[i];
            }
            
            flags[5] = abilityToUse.targetAll;
            flags[7] = true;
            
            battleState = 1;
            ParseTurnState();
        }

        private IEnumerator BasicEnemyAttackCommand()
        {
            abilityToUse = active.defaultAttack;
            flags[5] = active.defaultAttack.targetAll;
            
            List<int> possiblePositions = new List<int>();
            if (!partyField[0].deadTrigger) {possiblePositions.Add(1);}
            if (!partyField[1].deadTrigger) {possiblePositions.Add(2);}
            if (!partyField[2].deadTrigger) {possiblePositions.Add(3);}
            if (!partyField[3].deadTrigger) {possiblePositions.Add(5);}

            var rand = Random.Range(0, possiblePositions.Count);
            graphicalInterface.SetSelectorPosition(possiblePositions[rand]);
            battleState = 2;
            ParseTurnState();
            yield break;
        }
    }
}
