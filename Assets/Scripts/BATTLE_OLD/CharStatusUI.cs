using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharStatusUI : MonoBehaviour
{
    private const int retractedX = 32;
    private const int activeX = 142;
    public bool debugActiveTest = false;
    public Image hpBar;
    public Image mpBar;

    public void DrawUIElement(bool active, float hpRatio, float mpRatio)
    {
        var x = (active) ? activeX : retractedX;
        var thisTransform = transform;
        Vector3 currentPos = thisTransform.position;
        currentPos.x = x;
        thisTransform.position = currentPos;

        hpBar.fillAmount = hpRatio;
        mpBar.fillAmount = mpRatio;
    }
}
