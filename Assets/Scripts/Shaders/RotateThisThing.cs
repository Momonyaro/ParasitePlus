using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateThisThing : MonoBehaviour
{
    public Vector3 rot = Vector3.up;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rot * Time.deltaTime);
    }
}
