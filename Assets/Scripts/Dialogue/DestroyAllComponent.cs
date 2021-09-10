using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class DestroyAllComponent : DialogueComponent
    {
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            for (int i = 0; i < parent.components.Count; i++)
            {
                if (parent.components[i].reference.Equals(this.reference))
                {
                    break;
                }
                
                parent.DestroyComponentInstance(parent.components[i].reference);
            }
            
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
            return ComponentTypes.DESTROY_ALL;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }
    }
}
