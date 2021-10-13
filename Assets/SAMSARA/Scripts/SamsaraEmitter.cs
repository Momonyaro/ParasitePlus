using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAMSARA.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class SamsaraEmitter : MonoBehaviour
    {
        // Basically hook into the master and get the correct settings for the mixer event.

        public string eventReference = "";
        public bool playOnAwake = true;
        private SamsaraSoundStruct referencedEvent = null;
        private AudioSource _source = null;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        private void Start()
        {
            referencedEvent = SamsaraMaster.Instance.mixerAsset.GetSoundStructFromReference(eventReference, out bool success);
            if (!success)
                Debug.LogError($"[Samsara Emitter] : Failed to find event using reference <{eventReference}> on object [{gameObject.name}]");
            else
            {
                SetAudioSourceProperties(referencedEvent);
                if (playOnAwake)
                    _source.PlayDelayed(referencedEvent.delay);
            }
        }

        private void Update()
        {
            referencedEvent = SamsaraMaster.Instance.mixerAsset.GetSoundStructFromReference(referencedEvent.reference, out bool success);
        }

        /// <summary>
        /// This will play the currently set event. If the event is set to loop, consider using stop when you're tired of the sound looping.
        /// </summary>
        public void Play()
        {
            SetAudioSourceProperties(referencedEvent);
            _source.PlayDelayed(referencedEvent.delay);
        }

        /// <summary>
        /// Stop the currently playing event... Not much else to it.
        /// </summary>
        public void Stop()
        {
            _source.Stop();
        }

        /// <summary>
        /// Set the Audio Source properties to reflect the event we wish to play.
        /// </summary>
        /// <param name="soundStruct">The sound data.</param>
        private void SetAudioSourceProperties(SamsaraSoundStruct soundStruct)
        {
            _source.clip = soundStruct.audioClip;
            _source.volume = soundStruct.volume;
            _source.loop = soundStruct.loop;
            _source.pitch = Mathf.Clamp(soundStruct.pitch + Random.Range(-soundStruct.randomPitchOffset, soundStruct.randomPitchOffset), -3, 3);
        }
    }
}