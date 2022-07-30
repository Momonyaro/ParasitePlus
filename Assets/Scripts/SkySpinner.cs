using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySpinner : MonoBehaviour
{
    public Material skyboxMat;
    public float rotateSpeed;

    private float currentRot;

    private void Update()
    {
        currentRot += rotateSpeed * Time.deltaTime;
        currentRot %= 360;

        skyboxMat.SetFloat("_Rotation", currentRot);
    }
}
