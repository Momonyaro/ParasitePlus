using CORE;
using MOVEMENT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MapInteractable : MonoBehaviour
{
    public Vector3 dir = new Vector3();
    public float interactAngleOffset = 0.1f;
    public float interactRange = .5f;
    public Vector3 sensor = new Vector3();
    public bool triggerActive = false;
    public bool disabled = false;
    public bool activeState = false;
    public bool generateNewGUID = false;
    public string guid = "";
    public Color activeColor = Color.green;
    public Color disabledColor = Color.red;
    public string interactPromptMsg;
    public string onUseMsg;
    public string interactSfxReference;
    public AdressableLight[] adressableLights = new AdressableLight[0];
    public UnityEvent<bool> onActiveUpdate = new UnityEvent<bool>();
    public UnityEvent onEvent = new UnityEvent();


    [SerializeField] private float dot;
    private Transform playerTransform;


    private void OnValidate()
    {
        if (generateNewGUID)
        {
            guid = Guid.NewGuid().ToString();
            generateNewGUID = false;
        }
    }

    private void Start()
    {
        playerTransform = FindObjectOfType<FPSGridPlayer>().transform;
        DungeonManager dm = FindObjectOfType<DungeonManager>();
        CORE.MapManager mm = FindObjectOfType<CORE.MapManager>();

        if (dm.mapInteractables.Contains(this))
            return;

        dm.mapInteractables.Add(this);

        bool updateState = mm.PersistantStateExists(guid);
        Debug.Log("found earlier state in persistant?: " + updateState);

        if (updateState)
            activeState = mm.GetPersistantState(guid);

        UpdateLightStates();
        onActiveUpdate?.Invoke(activeState);
    }

    public void PlayEvent()
    {
        activeState = !activeState;

        UpdateLightStates();

        onActiveUpdate?.Invoke(activeState);
        onEvent?.Invoke();
    }

    bool CheckIfInteractable()
    {
        dot = Vector3.Dot(dir, playerTransform.forward);
        bool lookingAtTarget = false;
        bool inRange = false;

        dot += 1; // Should give us 0 since if we're looking at the door the dot product should be -1


        if (dot <= interactAngleOffset && dot >= -interactAngleOffset)
            lookingAtTarget = true;

        if (Vector3.Distance(sensor, playerTransform.position) < interactRange)
            inRange = true;

        return (inRange && lookingAtTarget);
    }

    public void UpdateLightStates()
    {
        Color selected = (activeState) ? activeColor : disabledColor;

        for (int i = 0; i < adressableLights.Length; i++)
        {
            adressableLights[i].SetLightColor(selected, selected);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (disabled) return;
        if (other.CompareTag("Player"))
        {
            triggerActive = (CheckIfInteractable());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (disabled) return;
        if (other.CompareTag("Player"))
        {
            triggerActive = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = triggerActive ? Color.green : Color.blue;
        Vector3 pos = sensor;
        Gizmos.DrawLine(pos, pos + dir * 0.3f);
    }
}
