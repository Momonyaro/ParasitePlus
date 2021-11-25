using System.Collections;
using System.Collections.Generic;
using SAMSARA.Scriptables;
using UnityEngine;

namespace SAMSARA
{
    public class SamsaraAudioChannel : MonoBehaviour
    {
        public List<AudioSource> tracks = new List<AudioSource>();
        public float channelVolume = 1;
        public AudioEvent storedAudioEvent = null;
        private Coroutine _audioRoutine = null;
        private bool playOutro = false;
        private float pitchOffset = 0;

        // This channel holds an audio event and plays it's tracks.
        
        // How do we play intros and outros?

        public void InitChannel(AudioEvent audioEvent)
        {
            storedAudioEvent = audioEvent;
            _audioRoutine = StartCoroutine(PlayAudioEnumerator());
        }

        public void KillChannel()
        {
            StopCoroutine(_audioRoutine);
            Destroy(gameObject);
        }

        private IEnumerator PlayAudioEnumerator()
        {
            EventTrackContainer trackContainer = storedAudioEvent.trackContainer;
            pitchOffset = Random.Range(-trackContainer.randomPitchOffset, trackContainer.randomPitchOffset);
            //Play the intro if it exists, otherwise just continuously update running tracks.

            AudioClip intro = trackContainer.introClip;
            WipeTracks();
            if (intro != null)
            {
                //Create a channel and play the intro until it reads as !isPlaying.
                AudioSource introSource = CreateTrack(intro);
                while (introSource.isPlaying)
                {
                    introSource.volume = trackContainer.volume * TryGetVolumeGroupValue(trackContainer.volumeGroupRef) * channelVolume;
                    introSource.pitch = trackContainer.pitch;
                    introSource.loop = false;
                    
                    yield return new WaitForEndOfFrame();
                }
                WipeTracks();
            }

            for (int i = 0; i < trackContainer.tracks.Count; i++)
            {
                CreateTrack(trackContainer.tracks[i].AudioClip);
            }

            bool running = true;

            while (running)
            {
                running = false;
                
                for (int i = 0; i < tracks.Count; i++)
                {
                    trackContainer = Samsara.Instance
                        .GetAudioEventFromReference(storedAudioEvent.reference, out bool foundSound).trackContainer;
                    tracks[i].volume = CalculateTrackVolume(i, trackContainer.volume * TryGetVolumeGroupValue(trackContainer.volumeGroupRef), trackContainer.lerpValue, 
                        trackContainer.ratio, trackContainer.trackLerpOverlap) * channelVolume;
                    tracks[i].pitch = trackContainer.pitch + pitchOffset;
                    tracks[i].loop = trackContainer.loop;
                    
                    if (tracks[i].isPlaying || tracks[i].loop)
                        running = true;
                    
                    yield return new WaitForEndOfFrame();
                }
            }
            
            KillChannel();
        }

        // This does not factor in the volume channels atm
        public float CalculateTrackVolume(int index, float volume, float currentLerp, float lerpRatio, float trackLerpOverlap)
        {
            float ratio = lerpRatio;
            float mult = currentLerp - ratio * index; // Center it around zero
            if (mult > -ratio * trackLerpOverlap && mult < ratio * trackLerpOverlap)
            {
                return volume * (1 - (Mathf.Abs(mult) / (ratio * trackLerpOverlap)));
            }
            else
            {
                return 0;
            }
            
        }

        public float TryGetVolumeGroupValue(string volumeGroupRef)
        {
            List<SamsaraVolumeGroup> volumeGroups = Samsara.Instance.mixerAsset.volumeGroups;

            for (int i = 0; i < volumeGroups.Count; i++)
            {
                if (volumeGroups[i].reference.Equals(volumeGroupRef))
                    return volumeGroups[i].groupVolume;
            }
            
            return 1;
        }

        public AudioSource CreateTrack(AudioClip clip)
        {
            GameObject track = new GameObject("AudioTrack",
                new []
                {
                    typeof(AudioSource)
                });

            track.transform.parent = transform;
            
            tracks.Add(track.GetComponent<AudioSource>());
            tracks[tracks.Count - 1].clip = clip;
            tracks[tracks.Count - 1].Play();

            return track.GetComponent<AudioSource>();
        }

        private void WipeTracks()
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i] != null)
                {
                    Destroy(tracks[i].gameObject);
                }
            }
            tracks.Clear();
        }
    }
}
