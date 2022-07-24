using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnLayerTrigger : MonoBehaviour
{
    public int newBtnLayer = 0;
    public bool changeScene = false;
    public bool parseDestination = true;
    public string newSceneVariable = "";

    public void TriggerEvent()
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();
        mapManager.currentSlimData.lastButtonLayer = newBtnLayer;

        if (changeScene)
        {

            mapManager.SwitchScene(newSceneVariable, parseDestination);
        }
    }
}
