using SAMSARA;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class PlaySoundComponent: DialogueComponent
    {
        public string soundEventRef;
        public bool stopEvent = false;
        public bool playAsMusic = false;
        public TransitionType transitionType;
        public float transitionTime = 0.3f;
        public bool playLayered = false;
        public int trackLayer = -1;
        
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            componentPrefab = null;

            bool success = false;
            if (playAsMusic)
            {
                if (!stopEvent)
                {
                    Samsara.Instance.MusicPlayLayered(soundEventRef, transitionType, transitionTime, out success);
                }
                else 
                    Samsara.Instance.MusicStopPlaying(transitionType, transitionTime, out success);
            }
            else
            {
                if (playLayered)
                {
                    Samsara.Instance.PlaySFXLayered(soundEventRef, out success);
                }
                else
                {
                    if (trackLayer < 0)
                    {
                        Samsara.Instance.PlaySFXRandomTrack(soundEventRef, out success);
                    }
                    else
                    {
                        Samsara.Instance.PlaySFXTrack(soundEventRef, trackLayer, out success);
                    }
                }
            }
        }

        public override void Update(out bool endOfLife)
        {
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
            return ComponentTypes.PLAY_SFX;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }
    }
}