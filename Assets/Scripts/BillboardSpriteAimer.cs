using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardSpriteAimer : MonoBehaviour
{
    public bool aimSprite = true;
    public bool lockToYAxis = false;
    public bool invertAngle = false;

    void Update()
    {
        AimSpriteAtCamera();
    }

    void AimSpriteAtCamera()
    {
        if (!aimSprite) return;
        GameObject currentCam = Camera.main.gameObject;
        Vector3 newRot = currentCam.transform.position - transform.position;
        newRot.x = 0; 
        newRot.z = 0;
        newRot.y = (!invertAngle) ? newRot.y : -newRot.y;
        Vector3 camPos = currentCam.transform.position;
        transform.LookAt(camPos - newRot);
        if (lockToYAxis)
        {
            Vector3 locked = new Vector3(-transform.rotation.x, 0, -transform.rotation.z);
            transform.Rotate(locked);
        }
    }
}
