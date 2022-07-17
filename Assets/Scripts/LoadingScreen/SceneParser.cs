using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneParser : MonoBehaviour
{
    static string[] sunriseScenes = new string[] 
    {
        "FiskekyrkaSunrise",
    };

    static string[] sundownScenes = new string[]
    {
        "FiskekyrkaSundown",
    };

    static string RandomSunrise => sunriseScenes[Random.Range(0, sunriseScenes.Length)];
    static string RandomSundown => sundownScenes[Random.Range(0, sundownScenes.Length)];

    public static void ParseSceneChange(string sceneName, out string slimDestination, out string destinationName)
    {
        //If the incoming scene is the map screen or one of the dungeons, we pick out the correct sunrise/sundown scene.

        slimDestination = destinationName = sceneName;
        string setNextScene = CORE.SlimComponent.Instance.ReadNonVolatileDesination;

        switch (sceneName)
        {
            case "Intro":
                if (setNextScene.Equals("Intro"))
                    break;
                destinationName = RandomSundown;
                break;

            case "MapMenu":
                if (setNextScene.Equals("MapMenu"))
                    break;
                destinationName = RandomSunrise;
                break;

            case "BeforeSwanDivision":
                if (setNextScene.Equals("BeforeSwanDivision"))
                    break;
                destinationName = RandomSundown;
                break;
        }
    }
}
