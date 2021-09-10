using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class DestroyComponent : DialogueComponent
    {
        public string refToDestroy = "";
        
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            parent.DestroyComponentInstance(refToDestroy);
            
            componentPrefab = null;
        }

        public override void Update(out bool endOfLife)
        {
            //Instantly destruct, we're doing all the logic during init.
            endOfLife = true;
        }

        public override void OnSubmitInput(InputAction.CallbackContext context)
        {
            // We're not using this
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
            return ComponentTypes.DESTROY;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }
    }
}
