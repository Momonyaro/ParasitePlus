using BattleSystem.UI;
using SAMSARA;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Player Win State", order = 1, menuName = "Battle States/Player Win State")]
    public class BattleSystemWinState : BattleSystemStateBase
    {
        public float randomXpRange = 0.1f;
        private bool doneShowingResults = false;
        private WinResultScreenUI winResultScreenUI;
        public AnimationCurve xpScalingCurve = new AnimationCurve();
        
        public override void Init()
        {
            winResultScreenUI = FindObjectOfType<WinResultScreenUI>();
            
            // SamsaraMaster.Instance.SetNextMusicTrackFromRef("_victoryTheme", out bool success);
            Samsara.Instance.MusicPlayLayered("_victoryTheme", TransitionType.CrossFade, 0.5f, out bool success);

            int[] enemyXp = new int[battleCore.partyField.Length];

            for (int p = 0; p < battleCore.partyField.Length; p++)
            {
                if (battleCore.partyField[p] == null) continue;
                for (int i = 0; i < battleCore.enemyField.Length; i++)
                {
                    if (battleCore.enemyField[i] == null) continue;
                    int normalXp = battleCore.enemyField[i].entityXp;
                    int lvlDiff = battleCore.enemyField[i].entityLevel - battleCore.partyField[p].entityLevel;
                    float scaling = xpScalingCurve.Evaluate(lvlDiff);

                    //Debug.Log($"Scaling p:{battleCore.partyField[p].entityName}, lvl diff: {lvlDiff}, scaling:{scaling.ToString("F1")}");

                    int scaledXp = Mathf.RoundToInt(normalXp * scaling);

                    enemyXp[p] += scaledXp;
                }
            }
            
            winResultScreenUI.transform.GetChild(0).gameObject.SetActive(true);
            winResultScreenUI.DisplayLevelUp(battleCore.GetPlayerParty(), enemyXp, randomXpRange);
            
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            doneShowingResults = winResultScreenUI.finished;
            endOfLife = false;
        }

        public override void Disconnect()
        {
            initialized = false;
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
            //On confirmation, we load out of combat to where we entered it.
            if (doneShowingResults)
            {
                battleCore.GoToDestinationScene();
            }
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {
            
        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {
            
        }
    }
}