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

    static string[] travelScenes = new string[]
    {
        "TramScene"
    };

    static string RandomSunrise => sunriseScenes[Random.Range(0, sunriseScenes.Length)];
    static string RandomSundown => sundownScenes[Random.Range(0, sundownScenes.Length)];
    static string RandomTravel  => travelScenes[Random.Range(0, travelScenes.Length)];

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
                if (setNextScene.Equals("CHRIS_STORE"))
                {
                    destinationName = RandomTravel;
                    break;
                }
                if (setNextScene.Equals("MeetSandra"))
                {
                    destinationName = RandomTravel;
                    break;
                }
                destinationName = RandomSunrise;
                break;

            case "BeforeSwanDivision":
                if (setNextScene.Equals("BeforeSwanDivision"))
                    break;
                destinationName = RandomSundown;
                break;
            case "ParkMeetGummo":
                if (setNextScene.Equals("ParkMeetGummo"))
                    break;
                destinationName = RandomTravel;
                break;
            case "CHRIS_STORE":
                if (setNextScene.Equals("CHRIS_STORE"))
                    break;
                destinationName = RandomTravel;
                break;
            case "MeetSandra":
                if (setNextScene.Equals("MeetSandra"))
                    break;
                destinationName = RandomTravel;
                break;
        }
    }
}
