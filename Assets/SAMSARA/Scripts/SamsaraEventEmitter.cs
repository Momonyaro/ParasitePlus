using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SamsaraEventEmitter : MonoBehaviour
{
    [Header("Event Settings")]
    [SerializeField] string eventReference = "";
    [SerializeField] bool playOnStart = false;

    [Header("SFX Settings")]
    [SerializeField] bool playLayered = false;
    [SerializeField] int layerIndex = 0;

    [Header("Music Settings")]
    [SerializeField] bool playAsMusic = false;
    [SerializeField] SAMSARA.TransitionType transitionType;
    [SerializeField] float transitionTime;


    private void Start()
    {
        if (playOnStart)
        {
            PlayEvent();
        }    
    }

    public void PlayEvent(string eventReference)
    {
        this.eventReference = eventReference;

        PlayEvent();
    }

    public void PlayEvent()
    {
        bool success = false;

        if (playAsMusic)
            SAMSARA.Samsara.Instance.MusicPlayLayered(eventReference, transitionType, transitionTime, out success);
        else
        {
            if (playLayered)
                SAMSARA.Samsara.Instance.PlaySFXLayered(eventReference, out success);
            else if (layerIndex < 0)
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack(eventReference, out success);
            else
                SAMSARA.Samsara.Instance.PlaySFXTrack(eventReference, layerIndex, out success);
        }
    }

    public void StopEvent()
    {
        bool success = false;

        if (playAsMusic)
            SAMSARA.Samsara.Instance.MusicStopPlaying(SAMSARA.TransitionType.Cut, 0, out success);
        else
            SAMSARA.Samsara.Instance.StopSFXEvent(eventReference, out success);
    }
}
