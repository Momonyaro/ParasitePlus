using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SAMSARA.Scripts
{
    public class SamsaraPlayer : MonoBehaviour
    {
        //Here is where we host the channel management.
        public int sfxChannelCount = 8;

        public List<AudioSource> sfxAudioChannels;
        public SamsaraTwinChannel musicTwinChannel;
        public SamsaraTwinChannel ambianceTwinChannel;

        private const string PlayingPrefix = "Playing -> ";

        private void Awake()
        {
            sfxAudioChannels = new List<AudioSource>();
            for (int i = 0; i < sfxChannelCount; i++)
            {
                sfxAudioChannels.Add(CreateAudioChannel(transform));
            }

            for (int i = 0; i < sfxAudioChannels.Count; i++)
            {
                sfxAudioChannels[i].playOnAwake = false;
                sfxAudioChannels[i].loop = false;
            }

            CreateMusicChannel(transform, out musicTwinChannel);
            CreateAmbientChannel(transform, out ambianceTwinChannel);
        }

        private void Update()
        {
            for (int i = 0; i < sfxAudioChannels.Count; i++)
            {
                if (!sfxAudioChannels[i].isPlaying)
                    sfxAudioChannels[i].name = "Not Playing...";
            }
        }

        /// <summary>
        /// Please consider using SamsaraMaster.Instance.PlaySFXFromReference() instead.
        /// </summary>
        public void PlayOnSFXChannel(SamsaraSoundStruct sound, out bool success)
        {
            //Debug.Log("Sources on this mono:" + this + " count for:" + SamsaraMaster.Instance.samsaraPlayer.sfxAudioChannels.Count);
            for (int i = 0; i < sfxAudioChannels.Count; i++)
            {
                if (!sfxAudioChannels[i].isPlaying)
                {
                    sfxAudioChannels[i].clip = sound.audioClip;
                    sfxAudioChannels[i].loop = sound.loop;
                    sfxAudioChannels[i].volume = sound.volume * SamsaraMaster.Instance.mixerAsset.masterVolume;
                    sfxAudioChannels[i].pitch = Mathf.Clamp(sound.pitch + Random.Range(-sound.randomPitchOffset, sound.randomPitchOffset), -3, 3);
                    sfxAudioChannels[i].name = PlayingPrefix + sound.reference;

                    sfxAudioChannels[i].PlayDelayed(sound.delay + Random.Range(0, sound.randomDelayOffset));

                    success = true;
                    return;
                }
            }
            success = false;
        }

        /// <summary>
        /// Please consider using SamsaraMaster.Instance.StopSFXFromReference() instead.
        /// </summary>
        public void StopSFX(string reference, out bool success)
        {
            success = false;
            for (int i = 0; i < sfxAudioChannels.Count; i++)
            {
                if (sfxAudioChannels[i].isPlaying)
                {
                    string croppedName = sfxAudioChannels[i].name.Replace(PlayingPrefix, "");
                    if (croppedName.Equals(reference))
                    {
                        sfxAudioChannels[i].Stop();

                        success = true;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Please consider using SamsaraMaster.Instance.SetNextMusicTrackFromRef() instead.
        /// </summary>
        public void SetInactiveMusicTrack(SamsaraSoundStruct soundStruct)
        {
            musicTwinChannel.SetInactiveChannelTrack(soundStruct);
        }

        /// <summary>
        /// Please consider using SamsaraMaster.Instance.SetNextMusicTrackFromRef() instead.
        /// </summary>
        public void SwapActiveMusicTrack(SamsaraTwinChannel.TransitionTypes transitionTypes, float transitionDuration, out bool success)
        {
            success = false;
            switch (transitionTypes)
            {
                case SamsaraTwinChannel.TransitionTypes.CrossFade:
                    StartCoroutine(musicTwinChannel.CrossFadeEnumerator(transitionDuration));
                    success = true;
                    break;
                case SamsaraTwinChannel.TransitionTypes.SmoothFade:
                    StartCoroutine(musicTwinChannel.SmoothFadeEnumerator(transitionDuration));
                    break;
                default:
                    StartCoroutine(musicTwinChannel.CrossFadeEnumerator(0.0f)); // Simulates cut by instantly switching.
                    break;
            }
        }

        /// <summary>
        /// Please consider using SamsaraMaster.Instance.SetNextAmbientTrackFromRef() instead.
        /// </summary>
        public void SetInactiveAmbianceTrack(SamsaraSoundStruct soundStruct)
        {
            ambianceTwinChannel.SetInactiveChannelTrack(soundStruct);
        }

        /// <summary>
        /// Please consider using SamsaraMaster.Instance.SwapAmbientTrack() instead.
        /// </summary>
        public void SwapActiveAmbientTrack(SamsaraTwinChannel.TransitionTypes transitionTypes, float transitionDuration, out bool success)
        {
            success = false;
            switch (transitionTypes)
            {
                case SamsaraTwinChannel.TransitionTypes.CrossFade:
                    StartCoroutine(ambianceTwinChannel.CrossFadeEnumerator(transitionDuration));
                    success = true;
                    break;
                case SamsaraTwinChannel.TransitionTypes.SmoothFade:
                    StartCoroutine(ambianceTwinChannel.SmoothFadeEnumerator(transitionDuration));
                    break;
                default:
                    StartCoroutine(ambianceTwinChannel.CrossFadeEnumerator(0.0f)); // Simulates cut by instantly switching.
                    break;
            }
        }





        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private AudioSource CreateAudioChannel(Transform parent)
        {
            GameObject newChannel = new GameObject("Audio Channel", new[]
            {
                typeof(AudioSource)
            });

            newChannel.transform.parent = parent;

            return newChannel.GetComponent<AudioSource>();
        }

        private void CreateMusicChannel(Transform parent, out SamsaraTwinChannel twinChannel)
        {
            GameObject host = new GameObject("Music Channels", new[]
            {
                typeof(SamsaraTwinChannel)
            });

            twinChannel = host.GetComponent<SamsaraTwinChannel>();
            host.transform.parent = parent.transform;

            GameObject channelA = new GameObject("Channel A", new[]
            {
                typeof(AudioSource)
            });

            channelA.transform.parent = host.transform;

            GameObject channelB = new GameObject("Channel B", new[]
            {
                typeof(AudioSource)
            });

            channelB.transform.parent = host.transform;
        }

        private void CreateAmbientChannel(Transform parent, out SamsaraTwinChannel twinChannel)
        {
            GameObject host = new GameObject("Ambient Channels", new[]
            {
                typeof(SamsaraTwinChannel)
            });

            twinChannel = host.GetComponent<SamsaraTwinChannel>();
            host.transform.parent = parent.transform;

            GameObject channelA = new GameObject("Channel A", new[]
            {
                typeof(AudioSource)
            });

            channelA.transform.parent = host.transform;

            GameObject channelB = new GameObject("Channel B", new[]
            {
                typeof(AudioSource)
            });

            channelB.transform.parent = host.transform;
        }
    }
}