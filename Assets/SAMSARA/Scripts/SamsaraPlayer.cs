using System.Collections;
using System.Collections.Generic;
using SAMSARA.Scriptables;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAMSARA
{
    public enum TransitionType
    {
        Cut,
        CrossFade,
        SmoothFade,
        OutroToIntro,
    }
    
    public class SamsaraPlayer : MonoBehaviour
    {
        // We need this class to manage the music and ambiance that's being played.
        public SamsaraAudioChannel currentMusicChannel;
        private bool _transitionInProgress = false;
        public SamsaraAudioChannel currentAmbianceChannel;

        public void MusicPlayNext(AudioEvent audioEvent, TransitionType transitionType, float transitionDuration, out bool success)
        {
            success = !_transitionInProgress;

            switch (transitionType)
            {
                case TransitionType.Cut:
                    StartCoroutine(CrossFadeEnumerator(audioEvent, 0));
                    break;
                case TransitionType.CrossFade:
                    StartCoroutine(CrossFadeEnumerator(audioEvent, transitionDuration));
                    break;
                case TransitionType.SmoothFade:
                    StartCoroutine(SmoothFadeEnumerator(audioEvent, transitionDuration));
                    break;
                case TransitionType.OutroToIntro:
                    StartCoroutine(IntroOutroFadeEnumerator(audioEvent, transitionDuration));
                    break;
            }

            // Use a transition IEnumerator to swap to a new track.
            // The enumerators should according to the type, fade out the existing track (if one is set)
            //     and then fade in the new track (if one is set).
        }

        public void MusicFadeOut(TransitionType transitionType, float transitionDuration, out bool success)
        {
            success = !_transitionInProgress;

            switch (transitionType)
            {
                case TransitionType.Cut:
                    StartCoroutine(CrossFadeEnumerator(null, 0));
                    break;
                case TransitionType.CrossFade:
                    StartCoroutine(CrossFadeEnumerator(null, transitionDuration));
                    break;
                case TransitionType.SmoothFade:
                    StartCoroutine(SmoothFadeEnumerator(null, transitionDuration));
                    break;
                case TransitionType.OutroToIntro:
                    StartCoroutine(IntroOutroFadeEnumerator(null, transitionDuration));
                    break;
            }

            // Use a transition IEnumerator to swap to a new track.
            // The enumerators should according to the type, fade out the existing track (if one is set)
            //     and then fade in the new track (if one is set).
        }

        private IEnumerator CrossFadeEnumerator(AudioEvent nextEvent, float transitionDuration)
        {
            if (_transitionInProgress) yield break;
            
            _transitionInProgress = true;
            SamsaraAudioChannel current = currentMusicChannel;
            SamsaraAudioChannel next = null;

            float lastVolume = 0;
            float nextVolume = 0;

            if (nextEvent != null)
            {
                next = CreateAudioChannel(nextEvent);
                nextVolume = next.channelVolume;
                next.channelVolume = 0;
            }

            if (current != null)
            {
                lastVolume = current.channelVolume;
            }

            float timePassed = 0;
            while (timePassed < transitionDuration)
            {
                float percentage = timePassed / transitionDuration;

                if (current != null)
                    current.channelVolume = lastVolume * (1.0f - percentage); // From high to 0
                if (next != null)
                    next.channelVolume = nextVolume * (0.0f + percentage); // From 0 to high
                
                timePassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (next != null)
                next.channelVolume = nextVolume;
            
            if (current != null)
                current.KillChannel();

            currentMusicChannel = next;
            
            _transitionInProgress = false;
            
            yield break;
        }
        
        private IEnumerator SmoothFadeEnumerator(AudioEvent nextEvent, float transitionDuration)
        {
            if (_transitionInProgress) yield break;
            
            _transitionInProgress = true;
            SamsaraAudioChannel current = currentMusicChannel;
            SamsaraAudioChannel next = null;
            if (nextEvent != null)
                next = CreateAudioChannel(nextEvent);

            float lastVolume = 0;
            float nextVolume = 0;

            if (current != null)
            {
                lastVolume = current.channelVolume;
            }
            
            if (next != null)
            {
                nextVolume = next.channelVolume;
                next.channelVolume = 0;
            }

            float timePassed = 0;
            float halfTime = transitionDuration * 0.5f;
            if (current != null)
            {
                while (timePassed < halfTime)
                {
                    timePassed += Time.deltaTime;
                    
                    float percentage = timePassed / halfTime;

                    current.channelVolume = lastVolume * (1.0f - percentage); // From high to 0
                    yield return new WaitForEndOfFrame();
                }

                current.channelVolume = 0;
            }

            timePassed = halfTime;
            if (next != null)
            {
                while (timePassed > 0)
                {
                    timePassed -= Time.deltaTime;
                    
                    float percentage = timePassed / halfTime;

                    next.channelVolume = nextVolume * (1.0f - percentage); // From 0 to high
                    yield return new WaitForEndOfFrame();
                }
            }

            next.channelVolume = nextVolume;

            if (next != null)
                next.channelVolume = nextVolume;
            
            if (current != null)
                current.KillChannel();

            currentMusicChannel = next;
            _transitionInProgress = false;
            
            yield break;
        }

        private IEnumerator IntroOutroFadeEnumerator(AudioEvent nextEvent, float deadAirDuration)
        {
            //trigger current music track to play it's outro, when the event dies
            // start the new track so that it's intro plays.
            
            yield break;
        }
        
        
        public SamsaraAudioChannel CreateAudioChannel(AudioEvent audioEvent)
        {
            string channelName = "Playing -> " + audioEvent.reference;
            
            GameObject channel = new GameObject(channelName,
                new []
                {
                    typeof(SamsaraAudioChannel)
                });

            channel.transform.parent = transform;
            channel.GetComponent<SamsaraAudioChannel>().InitChannel(audioEvent);
            return channel.GetComponent<SamsaraAudioChannel>();
        }

        public void DestroyAudioChannel(string reference)
        {
            SamsaraAudioChannel channel = TryGetAudioChannel(reference);

            if (channel == null) return;

            Destroy(channel.gameObject);
        }

        public SamsaraAudioChannel TryGetAudioChannel(string reference)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                SamsaraAudioChannel audioChannel = child.GetComponent<SamsaraAudioChannel>();

                if (audioChannel.storedAudioEvent.reference.Equals(reference))
                    return audioChannel;
            }
            return null;
        }
    }
}
