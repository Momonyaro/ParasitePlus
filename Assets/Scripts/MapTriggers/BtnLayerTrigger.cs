using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BtnLayerTrigger : MonoBehaviour
{
    public int newBtnLayer = 0;
    public bool changeScene = false;
    public bool useMapManager = true;
    public bool parseDestination = true;
    public string newSceneVariable = "";

    public void TriggerEvent()
    {
        if (useMapManager)
            CallWithMapManager();
        else
            ReadRawSlim();
    }

    private void CallWithMapManager()
    {
        CORE.MapManager mapManager = FindObjectOfType<CORE.MapManager>();
        mapManager.currentSlimData.lastButtonLayer = newBtnLayer;

        if (changeScene)
        {
            mapManager.SwitchScene(newSceneVariable, parseDestination);
        }
    }

    private void ReadRawSlim()
    {
        CORE.SlimComponent.Instance.SetNonVolatileBtnLayer(newBtnLayer);

        if (changeScene)
        {
            if (parseDestination)
            {
                SceneParser.ParseSceneChange(newSceneVariable, out string slimDestination, out string destination);
                CORE.SlimComponent.Instance.SetNonVolatileDestination(slimDestination);
                UnityEngine.SceneManagement.SceneManager.LoadScene(destination);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(newSceneVariable);
            }
        }
    }
}
