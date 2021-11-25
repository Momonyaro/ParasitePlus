using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAMSARA.Scriptables
{
    [CreateAssetMenu(fileName = "Mixer Asset", menuName = "SAMSARA/Mixer Asset", order = 0)]
    public class MixerAssetScriptable : ScriptableObject
    {
        //Have the master copy the contents of this into a dictionary.
        public List<AudioEvent> audioEvents = new List<AudioEvent>();
        public List<SamsaraVolumeGroup> volumeGroups = new List<SamsaraVolumeGroup>()
        {
            new SamsaraVolumeGroup()
            {
                reference = "_general",
                groupVolume = 1
            }
        };

        private void OnValidate()
        {
            for (int i = 0; i < audioEvents.Count; i++)
            {
                audioEvents[i].trackContainer.UpdateTrackContainer();
            }
        }
    }

    [Serializable]
    public class AudioEvent
    {
        public string reference;
        public EventTrackContainer trackContainer;
        //Here we need to keep the reference to the clips/layers with their intros and stuff.

        public AudioEvent()
        {
            reference = "_newClip";
            trackContainer = new EventTrackContainer();
        }
    }

    [Serializable]
    public class EventTrackContainer
    {
        [Range(0.0f, 1.0f)]public float lerpValue = 0;
        [SerializeField] public float ratio = 0;
        [Range(0.5f, 1.0f)] public float trackLerpOverlap;
        public string volumeGroupRef = "_general";
        
        public float volume;
        public float pitch; 
        public float randomPitchOffset;
        public float delay;
        public bool loop;
        
        public AudioClip introClip;
        public AudioClip outroClip;
        
        public List<TrackData> tracks = new List<TrackData>();

        //How we wanna do this?
        //A list of tracks? Spawn a player for each and have some magic variable to lerp between volumes?

        // Lerp range 0 - 1
        // Divide each track intro ranges of size 1 / tracks.Count
        // We can then give them the value of ( i * (1 / tracks.Count))
        // Range from list of 5 would look like : 0, 0.2, 0.4, 0.6, 0.8, 1
        // We can now limit and say that if the current lerp amount is within (1 / tracks.Count)
        //     of their number, we set it's volume to the tracks audio * where the lerp number is between it's
        //     number and (1 / tracks.Count) +- of that number.
        // An option could exist to allow overlapping tracks or to disallow it.

        // This implementation is nice because we can now also say, play track 3 and we will get
        //     3 * (1 / tracks.Count) = 0.6 which would make track 3 play at it's default volume.

        public void UpdateTrackContainer()
        {
            this.ratio = (tracks.Count == 0) ? 0 : (1 / (float)tracks.Count);
        }

        public EventTrackContainer()
        {
            this.volume = 1;
            this.pitch = 1;
            this.randomPitchOffset = 0;
            this.delay = 0;
            this.trackLerpOverlap = 1.0f;
            this.ratio = (tracks.Count == 0) ? 0 : (1 / (float)tracks.Count);
        }
    }
    
    [Serializable]
    public class TrackData
    {
        public AudioClip AudioClip;
    }
}
