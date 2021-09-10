using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class WaitComponent : DialogueComponent
    {

        public float waitTime = 1.0f;
        private float timer = 0;
        
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            componentPrefab = objectPrefab;
            timer = waitTime;
        }

        public override void Update(out bool endOfLife)
        {
            timer -= Time.deltaTime;
            if (timer <= 0.0f)
                endOfLife = true;
            else
                endOfLife = false;
        }

        public override void OnSubmitInput(InputAction.CallbackContext context)
        {
            timer = 0;
        }

        public override void OnCancelInput(InputAction.CallbackContext context)
        {
            // We're not using this
        }

        public override void OnMoveInput(InputAction.CallbackContext context)
        {
            // We're not using this
        }

        public override ComponentTypes GetComponentType()
        {
            return ComponentTypes.WAIT;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }
    }
}
