using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAMSARA.Scripts
{
    public class SamsaraMaster : MonoBehaviour
    {
        //This script is the glue, here we hold the current mixer asset and also the player

        //The channels I'm imagining is:
        // * user definable sound channels (recommend 4 channels)
        // * 2 ambient channels (it's made to have one active with the possibility to tween and perhaps blend between the two channels)
        // * 2 music channels (similar reason to the ambient channels, to enable cross-fading and smooth fading.)

        //Accessible as a singleton for easy system integration.
        // runtime player creation? (perhaps just nope and say user error? (nah, that's cringe. Create an explicit method for it instead.))
        // 3d audio? (because of how SAMSARA works, it's perhaps not going to be possible.)
        //  * Actually, if I instead add something that hooks into the current mixer asset and plays it on a audio source.
        //    This would get us 3d audio similar to how FMod works. Reverb would also be done through Unity's own Reverb Zones in this case.

        public static SamsaraMaster Instance { get; private set; }

        public bool runInDontDestroy = false;
        public SamsaraPlayer samsaraPlayer = null;
        public SamsaraMixerAsset mixerAsset = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (runInDontDestroy)
                DontDestroyOnLoad(gameObject);
            
            Instance = this;
            if (mixerAsset != null)
            {
                mixerAsset.PopulateDictionary();
            }
        }
        
        
        private void Start()
        {
            // MUSIC DEMO CODE
#if false
            SamsaraMaster.Instance.SetInactiveMusicTrackFromRef("_demo", out bool success);
            
            SamsaraTwinChannel.TransitionTypes transitionType = SamsaraTwinChannel.TransitionTypes.CrossFade;
            SamsaraMaster.Instance.samsaraPlayer.SwapActiveMusicTrack(transitionType, 1.0f, out success);
            if (!success)
            {
                Debug.Log("Sadge :(");
            }
#endif

            // SFX DEMO CODE
#if false
            SamsaraMaster.Instance.PlaySFXFromReference("_demo", out bool success);
            if (!success)
            {
                Debug.Log("Sadge :(");
            }
#endif
        }


        /// <summary>
        /// Play a 2D sound using the available sound channels (the number of channels can be decided on the samsaraPlayer).
        /// If no channel is available, we simply don't play the sound. Consider increasing the amount of total channels
        /// if you are running into problems.
        /// </summary>
        /// <param name="reference">The reference to the event that you wish to play.</param>
        /// <param name="success">Returns whether we succeeded in setting the sound.</param>
        public void PlaySFXFromReference(string reference, out bool success)
        {
            success = false;
            if (mixerAsset == null) { return; }

            SamsaraSoundStruct soundStruct = mixerAsset.GetSoundStructFromReference(reference, out success);

            if (success && samsaraPlayer != null)
            {
                Instance.samsaraPlayer.PlayOnSFXChannel(soundStruct, out success);
            }
            else
            {
                success = false;
            }
            //Debug.Log("Audio:" + success);
        }

        /// <summary>
        /// Cancel the playback of a currently active 2D sound. If the sound is not found, the request is ignored.
        /// </summary>
        /// <param name="reference">The reference to the event that you wish to cancel.</param>
        /// <param name="success">Returns whether we succeeded in canceling the sound.</param>
        public void StopSFXFromReference(string reference, out bool success)
        {
            success = false;

            if (samsaraPlayer != null)
            {
                samsaraPlayer.StopSFX(reference, out success);
            }
            else
            {
                success = false;
            }
        }

        /// <summary>
        /// Set the next music track to play through using a reference to the event. You can find the reference in the mixer asset.
        /// Play this track by using SwapMusicTrack().
        /// </summary>
        /// <param name="reference">This is the event's reference string, found in the current mixer asset.</param>
        /// <param name="success">Returns whether we succeeded in setting the sound.</param>
        public void SetNextMusicTrackFromRef(string reference, out bool success)
        {
            success = false;
            if (mixerAsset == null) { return; }

            if (samsaraPlayer != null)
            {
                samsaraPlayer.SetInactiveMusicTrack(mixerAsset.GetSoundStructFromReference(reference, out success));
            }
            else
            {
                success = false;
            }
        }

        /// <summary>
        /// Swaps to the inactive music track using different transitions, if swapping to an empty track it will act as a fade-out.
        /// If you swap from an empty track it will act as a fade-in.
        /// </summary>
        /// <param name="transitionType">Select the method of transition, Cross-fade fades out the old clip while it fades in the new one.
        /// Smooth-fade first fades out the old clip and then fades in the new one. Cut immediately swaps between the old and new clip with no transition.</param>
        /// <param name="transitionDuration">This controls the total time for the transition (cut ignores the duration and immediately cuts).
        /// Smooth-fade takes <b>(transitionDuration * 0.5)</b> to fade out and the same time to fade in the new clip.</param>
        /// <param name="success">Returns whether we succeeded in setting the sound.</param>
        public void SwapMusicTrack(SamsaraTwinChannel.TransitionTypes transitionType, float transitionDuration, out bool success)
        {
            samsaraPlayer.SwapActiveMusicTrack(transitionType, transitionDuration, out success);
        }

        /// <summary>
        /// Set the next ambient track to play through using a reference to the event. You can find the reference in the mixer asset.
        /// Play this track by using SwapAmbientTrack().
        /// </summary>
        /// <param name="reference">This is the event's reference string, found in the current mixer asset.</param>
        /// <param name="success">Returns whether we succeeded in setting the sound.</param>
        public void SetNextAmbientTrackFromRef(string reference, out bool success)
        {
            success = false;
            if (mixerAsset == null) { return; }

            if (samsaraPlayer != null)
            {
                samsaraPlayer.SetInactiveAmbianceTrack(mixerAsset.GetSoundStructFromReference(reference, out success));
            }
            else
            {
                success = false;
            }
        }

        /// <summary>
        /// Swaps to the inactive ambient track using different transitions, if swapping to an empty track it will act as a fade-out.
        /// If you swap from an empty track it will act as a fade-in.
        /// </summary>
        /// <param name="transitionType">Select the method of transition, Cross-fade fades out the old clip while it fades in the new one.
        /// Smooth-fade first fades out the old clip and then fades in the new one. Cut immediately swaps between the old and new clip with no transition.</param>
        /// <param name="transitionDuration">This controls the total time for the transition (cut ignores the duration and immediately cuts).
        /// Smooth-fade takes <b>(transitionDuration * 0.5)</b> to fade out and the same time to fade in the new clip.</param>
        /// <param name="success">Returns whether we succeeded in setting the sound.</param>
        public void SwapAmbientTrack(SamsaraTwinChannel.TransitionTypes transitionType, float transitionDuration, out bool success)
        {
            samsaraPlayer.SwapActiveAmbientTrack(transitionType, transitionDuration, out success);
        }
    }
}
