using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateObjTimed : MonoBehaviour
{
    public GameObject[] objects = new GameObject[0];
    public bool nextObjActiveState = true;
    public float timeToSetState = 1;

    private void Awake()
    {
        StartCoroutine(WaitToActivate().GetEnumerator());
    }

    IEnumerable WaitToActivate()
    {
        yield return new WaitForSeconds(timeToSetState);
        for (int i = 0; i < objects.Length; i++)
        {
            objects[i].SetActive(nextObjActiveState);
        }
    }
}
