using BattleSystem.UI;
using SAMSARA;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BattleSystem.States
{
    [CreateAssetMenu(fileName = "Player Loss State", order = 1, menuName = "Battle States/Player Loss State")]
    public class BattleSystemLossState : BattleSystemStateBase
    {
        public override void Init()
        {
            battleCore.GoToGameOverScene();
            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = false;
        }

        public override void Disconnect()
        {
            initialized = false;
        }

        public override void OnSubmitButton(InputAction.CallbackContext obj)
        {
        }

        public override void OnCancelButton(InputAction.CallbackContext obj)
        {

        }

        public override void OnMoveAxis(InputAction.CallbackContext obj)
        {

        }
    }
}
