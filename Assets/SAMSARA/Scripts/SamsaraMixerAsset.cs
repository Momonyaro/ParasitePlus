using System.Collections.Generic;
using UnityEngine;

namespace SAMSARA.Scripts
{
    [CreateAssetMenu(fileName = "Mixer Asset", menuName = "SAMSARA/Mixer Asset", order = 0)]
    public class SamsaraMixerAsset : ScriptableObject
    {
        [Range(0, 1)] public float masterVolume = 1.0f;
        public List<SamsaraSoundStruct> events = new List<SamsaraSoundStruct>();
        private Dictionary<string, SamsaraSoundStruct> eventdic = new Dictionary<string, SamsaraSoundStruct>();

        public void PopulateDictionary()
        {
            eventdic = new Dictionary<string, SamsaraSoundStruct>();
            foreach (var item in events)
            {
                eventdic.Add(item.reference, item);
            }
        }

        /// <summary>
        /// This fetches a <b>SamsaraSoundStruct</b> from a reference. If no struct is found, success will be false.
        /// </summary>
        /// <param name="reference">The reference of the event you wish to fetch.</param>
        /// <param name="success">The result of finding the event.</param>
        /// <returns></returns>
        public SamsaraSoundStruct GetSoundStructFromReference(string reference, out bool success)
        {
            //Checks Dictionary
            if (eventdic.ContainsKey(reference))
            {
                success = true;
                return eventdic[reference];
            }

            //Dictionary does not contain refrence, Checks list
            success = false;
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].reference.Equals(reference))
                {
                    Debug.LogWarning("List modified without updating dictionary!");
                    success = true;
                    return events[i];
                }
            }

            //Does not contain audio
            Debug.LogWarning("Couldn't find AUDIO Refrence:" + reference);
            return null;
        }

        /// <summary>
        /// Overwrite the data of an event using it's reference.
        /// </summary>
        /// <param name="reference">The reference of the event you wish to overwrite.</param>
        /// <param name="data">The new event data.</param>
        /// <param name="success">Did we succeed finding an event to overwrite?</param>
        public void SetSoundStructProperties(string reference, SamsaraSoundStruct data, out bool success)
        {
            //Checks Dictionary
            if (eventdic.ContainsKey(reference))
            {
                success = true;
                eventdic.Remove(reference);
                eventdic.Add(reference, data);
                return;
            }

            //Dictionary does not contain refrence, Checks list
            success = false;
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].reference.Equals(reference))
                {
                    Debug.LogWarning("List modified without updating dictionary!");
                    success = true;
                    events[i] = data;
                    events[i].reference = reference; // In case it got renamed.
                    return;
                }
            }
        }
    }

    [System.Serializable]
    public class SamsaraSoundStruct
    {
        public string reference = "_newClip";
        public AudioClip audioClip;
        public bool loop = false;
        [Range(0, 1)] public float volume = 1.0f;
        [Range(-2, 2)] public float pitch = 0.0f;
        [Range(0, 1)] public float randomPitchOffset = 0.0f;
        public float delay = 0.0f;
        [Range(0, 1)] public float randomDelayOffset = 0.0f;
    }
}