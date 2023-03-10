using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectTrigger : MonoBehaviour
{
    public List<ToggleState> toggleStates = new List<ToggleState>();

    public void Trigger()
    {
        toggleStates.ForEach(s => { s.go.SetActive(s.isActive); });
    }

    [System.Serializable]
    public struct ToggleState
    {
        public GameObject go;
        public bool isActive;
    }
}
