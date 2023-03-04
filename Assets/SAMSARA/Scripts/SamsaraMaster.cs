using System;
using System.Collections;
using System.Collections.Generic;
using SAMSARA.Scriptables;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAMSARA
{
    public static class Samsara
    {
        private static SamsaraMaster _instance;
        
        public static SamsaraMaster Instance
        {
            get
            {
                if (_instance == null)
                {
                    for (int i = 0; i < GameObject.FindObjectsOfType<SamsaraMaster>().Length; i++)
                    {
                        GameObject.DestroyImmediate(GameObject.FindObjectsOfType<SamsaraMaster>()[i].gameObject, false);
                    }
                    
                    _instance = new GameObject("SamsaraMaster", new[]
                    {
                        typeof(SamsaraPlayer),
                        typeof(SamsaraMaster),
                    }).GetComponent<SamsaraMaster>();
                }

                return _instance;
            }
            private set => _instance = value;
        }
    }
    
    
    public class SamsaraMaster : MonoBehaviour
    {
        public MixerAssetScriptable mixerAsset;
        private SamsaraPlayer _samsaraPlayer;

        public Dictionary<string, AudioEvent> eventData;
        public List<string> eventGroups = new List<string>();
        private HashSet<string> _enquedThisFrame = new HashSet<string>();
        //Here we are once again, this class again is responsible for the majority of user interaction.
        // Playing, swapping, stopping and modifying is all handled from here and sent to the correct components.

        private void Awake()
        {
            if (mixerAsset == null)
            {
                if (Resources.Load("_mixerAsset") != null)
                {
                    mixerAsset = Resources.Load<MixerAssetScriptable>("_mixerAsset");
                }
                else
                    return;
            }

            List<AudioEvent> cached = new List<AudioEvent>(mixerAsset.audioEvents);
            eventData = new Dictionary<string, AudioEvent>();

            for (int i = 0; i < mixerAsset.audioEvents.Count; i++)
            {
                eventData.Add(cached[i].reference, cached[i]);
            }

            eventGroups = new List<string>(mixerAsset.eventGroups);

            _samsaraPlayer = GetComponent<SamsaraPlayer>();
            
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            _enquedThisFrame.Clear();
        }

        public void PlaySFXLayered(string reference, out bool success)
        {
            success = false;

            if (_enquedThisFrame.Contains(reference)) return;

            AudioEvent fetched = GetAudioEventFromReference(reference, out bool foundEvent);
            if (foundEvent)
            {
                success = true;
                Debug.Log("Found event with reference: " + reference);
                _samsaraPlayer.CreateAudioChannel(fetched);
                _enquedThisFrame.Add(reference);
            }
        }

        public void PlaySFXTrack(string reference, int trackIndex, out bool success)
        {
            success = false;

            if (_enquedThisFrame.Contains(reference)) return;

            AudioEvent fetched = GetAudioEventFromReference(reference, out bool foundEvent);
            if (foundEvent)
            {
                success = true;
                int randomIndex = trackIndex;
                AudioEvent cropped = new AudioEvent()
                {
                    reference = fetched.reference,
                    trackContainer = new EventTrackContainer()
                    {
                        delay = fetched.trackContainer.delay,
                        introClip = fetched.trackContainer.introClip,
                        lerpValue = 0,
                        loop = fetched.trackContainer.loop,
                        outroClip = fetched.trackContainer.outroClip,
                        pitch = fetched.trackContainer.pitch,
                        randomPitchOffset = fetched.trackContainer.randomPitchOffset,
                        ratio = 1,
                        trackLerpOverlap = fetched.trackContainer.trackLerpOverlap,
                        tracks = new List<TrackData>() { fetched.trackContainer.tracks[randomIndex] },
                    }
                };
                cropped.trackContainer.UpdateTrackContainer();
                
                _samsaraPlayer.CreateAudioChannel(cropped);
                _enquedThisFrame.Add(reference);
            }
        }

        public void PlaySFXRandomTrack(string reference, out bool success)
        {
            success = false;
            if (reference == null || reference.Equals("")) return;
            if (_enquedThisFrame.Contains(reference)) return;

            AudioEvent fetched = GetAudioEventFromReference(reference, out bool foundEvent);
            if (foundEvent)
            {
                success = true;
                int randomIndex = Random.Range(0, fetched.trackContainer.tracks.Count);
                AudioEvent cropped = new AudioEvent()
                {
                    reference = fetched.reference,
                    trackContainer = new EventTrackContainer()
                    {
                        delay = fetched.trackContainer.delay,
                        introClip = fetched.trackContainer.introClip,
                        lerpValue = 0,
                        loop = fetched.trackContainer.loop,
                        outroClip = fetched.trackContainer.outroClip,
                        pitch = fetched.trackContainer.pitch,
                        randomPitchOffset = fetched.trackContainer.randomPitchOffset,
                        ratio = 1,
                        trackLerpOverlap = fetched.trackContainer.trackLerpOverlap,
                        tracks = new List<TrackData>() { fetched.trackContainer.tracks[randomIndex] },
                    }
                };
                cropped.trackContainer.UpdateTrackContainer();
                
                _samsaraPlayer.CreateAudioChannel(cropped);
                _enquedThisFrame.Add(reference);
            }
        }

        public void StopSFXEvent(string reference, out bool success)
        {
            _samsaraPlayer.DestroyAudioChannel(reference);
            success = true;
        }

        public void MusicPlayLayered(string reference, TransitionType transitionType, float transitionDuration, out bool success)
        {
            success = false;

            AudioEvent fetched = GetAudioEventFromReference(reference, out success);
            if (success)
            {
                _samsaraPlayer.MusicPlayNext(fetched, transitionType, transitionDuration, out success);
            }
        }

        public string GetMusicPlaying(out bool success)
        {
            success = false;
            return _samsaraPlayer.GetMusicPlayingRef(out success);
        }

        public void MusicStopPlaying(TransitionType transitionType, float transitionDuration, out bool success)
        {
            success = false;
            _samsaraPlayer.MusicFadeOut(transitionType, transitionDuration, out success);
        }

        public AudioEvent GetAudioEventFromReference(string reference, out bool success)
        {
            success = eventData.ContainsKey(reference);

            if (success)
            {
                return eventData[reference];
            }

            return null;
        }

        public void OverwriteAudioEvent(AudioEvent audioEvent, out bool success)
        {
            success = eventData.ContainsKey(audioEvent.reference);
            if (success)
            {
                eventData[audioEvent.reference] = audioEvent;
            }
        }
    }

    [System.Serializable]
    public class SamsaraVolumeGroup
    {
        public string reference = "_newGroup";
        public float groupVolume = 1;
    }
}
