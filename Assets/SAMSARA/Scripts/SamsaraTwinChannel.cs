using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAMSARA.Scripts
{
    public class SamsaraTwinChannel : MonoBehaviour
    {
        public enum Channels
        {
            ChannelA,
            ChannelB
        }

        public enum TransitionTypes
        {
            Cut,
            SmoothFade,
            CrossFade,
        }
        
        public AudioSource channelA;
        public AudioSource channelB;
        private Channels _activeChannel = Channels.ChannelB;
        private bool _transitionInProgress = false;

        private void Start()
        {
            //Assume normal structure!
            channelA = transform.GetChild(0).GetComponent<AudioSource>();
            channelB = transform.GetChild(1).GetComponent<AudioSource>();

            channelA.loop = true;
            channelB.loop = true;
        }

        /// <summary>
        /// Set the properties for the currently inactive channel in preparation to play it.
        /// </summary>
        /// <param name="soundStruct">The event data.</param>
        public void SetInactiveChannelTrack(SamsaraSoundStruct soundStruct)
        {
            if (soundStruct == null)
                return;
            
            AudioSource relevantChannel = (_activeChannel == Channels.ChannelA) ? channelB : channelA;

            relevantChannel.clip = soundStruct.audioClip;
            relevantChannel.loop = soundStruct.loop;
            relevantChannel.volume = soundStruct.volume * SamsaraMaster.Instance.mixerAsset.masterVolume;;
            relevantChannel.pitch = Mathf.Clamp(soundStruct.pitch + Random.Range(-soundStruct.randomPitchOffset, soundStruct.randomPitchOffset), -3, 3);
        }
        
        /// <summary>
        /// Clear the currently inactive track, this is useful if you want to fade out the currently playing track.
        /// </summary>
        public void ClearInactiveChannelTrack()
        {
            AudioSource relevantChannel = (_activeChannel == Channels.ChannelA) ? channelB : channelA;

            relevantChannel.clip = null;
            relevantChannel.loop = false;
            relevantChannel.volume = 0;
            relevantChannel.pitch = 1.0f;
        }

        
        /// <summary>
        /// This is the enumerator for a Cross-fade transition, this transition will during the time set by the <b>transitionDuration</b>
        /// fade out the currently active channel and fade in the inactive one before switching which channel is active and stopping.
        /// </summary>
        /// <param name="transitionDuration">The time that the cross-fade takes.</param>
        /// <returns></returns>
        public IEnumerator CrossFadeEnumerator(float transitionDuration)
        {
            if (_transitionInProgress) yield break;
            
            _transitionInProgress = true;
            AudioSource lastActive = (_activeChannel == Channels.ChannelA) ? channelA : channelB;
            AudioSource nextActive = (_activeChannel == Channels.ChannelA) ? channelB : channelA;

            float lastVolume = lastActive.volume;
            float nextVolume = nextActive.volume;

            nextActive.volume = 0;
            nextActive.Play();

            float timePassed = 0;
            while (timePassed < transitionDuration)
            {
                float percentage = timePassed / transitionDuration;

                lastActive.volume = lastVolume * (1.0f - percentage); // From high to 0
                nextActive.volume = nextVolume * (0.0f + percentage); // From 0 to high
                
                timePassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            nextActive.volume = nextVolume;

            lastActive.volume = 0;
            lastActive.Stop();

            _activeChannel = (_activeChannel == Channels.ChannelA) ? Channels.ChannelB : Channels.ChannelA;
            _transitionInProgress = false;
            
            yield break;
        }
        
        /// <summary>
        /// This is the enumerator for a Smooth-fade transition (that's actually linear), this transition will during the time set by the <b>(transitionDuration * 0.5)</b>
        /// fade out the currently active channel and then start fading in the inactive channel. Once this is done, swap what channel is set as the active one and stop the enumerator.
        /// </summary>
        /// <param name="transitionDuration">The time that the full smooth-fade takes</param>
        /// <returns></returns>
        public IEnumerator SmoothFadeEnumerator(float transitionDuration)
        {
            if (_transitionInProgress) yield break;
            
            _transitionInProgress = true;
            AudioSource lastActive = (_activeChannel == Channels.ChannelA) ? channelA : channelB;
            AudioSource nextActive = (_activeChannel == Channels.ChannelA) ? channelB : channelA;

            float lastVolume = lastActive.volume;
            float nextVolume = nextActive.volume;

            nextActive.volume = 0;
            nextActive.Play();

            float timePassed = 0;
            float halfTime = transitionDuration * 0.5f;
            while (timePassed < halfTime)
            {
                float percentage = timePassed / transitionDuration;

                lastActive.volume = lastVolume * (1.0f - percentage); // From high to 0
                
                timePassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            timePassed = halfTime;
            while (timePassed > 0)
            {
                float percentage = timePassed / transitionDuration;

                nextActive.volume = nextVolume * (1.0f - percentage); // From 0 to high
                
                timePassed -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            nextActive.volume = nextVolume;

            lastActive.volume = 0;
            lastActive.Stop();

            _activeChannel = (_activeChannel == Channels.ChannelA) ? Channels.ChannelB : Channels.ChannelA;
            _transitionInProgress = false;
            
            yield break;
        }
    }
}