using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class SpawnBgObjectComponent : DialogueComponent
    {
        public Sprite backgroundImage;
        
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            componentPrefab = objectPrefab;
        }

        public override void Update(out bool endOfLife)
        {
            BackgroundReciever obj = GetCurrentInstance().GetComponent<BackgroundReciever>();

            if (obj != null)
            {
                obj.GetComponent<BackgroundReciever>().backgroundImage.sprite = backgroundImage;
            }

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
            return ComponentTypes.SPAWN_OBJECT_BG;
        }
        
        public override bool PlaceInBackground()
        {
            return true;
        }
    }
}