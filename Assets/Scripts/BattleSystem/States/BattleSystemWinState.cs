using BattleSystem.UI;
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
        
        public override void Init()
        {
            winResultScreenUI = FindObjectOfType<WinResultScreenUI>();
            
            // SamsaraMaster.Instance.SetNextMusicTrackFromRef("_victoryTheme", out bool success);
            // if (success)
            //     SamsaraMaster.Instance.SwapMusicTrack(SamsaraTwinChannel.TransitionTypes.SmoothFade, 0.2f, out success);

            int enemyXp = 0;

            for (int i = 0; i < battleCore.enemyField.Length; i++)
            {
                if (battleCore.enemyField[i] == null) continue;

                enemyXp += battleCore.enemyField[i].entityXp;
            }
            
            winResultScreenUI.transform.GetChild(0).gameObject.SetActive(true);
            winResultScreenUI.DisplayLevelUp(battleCore.partyField, enemyXp, randomXpRange);
            
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