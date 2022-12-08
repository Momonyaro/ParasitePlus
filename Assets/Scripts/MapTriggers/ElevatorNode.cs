using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorNode : MonoBehaviour
{
    MOVEMENT.FPSGridPlayer player;
    public FloorData[] levels = new FloorData[0];
    public int currentFloor = 0;
    private bool travelling = false;
    private bool shaking = false;
    public float timePerFloor = 1.5f;

    private void Awake()
    {
        player = FindObjectOfType<MOVEMENT.FPSGridPlayer>();
        travelling = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PrepareForElevator();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.lockPlayer = true;
        }
    }

    private void PrepareForElevator()
    {
        player.TurnAround();

        //Feed floor data here to create menu
        FindObjectOfType<ElevatorMenu>().CreateMenu(node: this);

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].destinationScene.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name))
            {
                FindObjectOfType<ElevatorMenu>().SetActiveFloor(i);
                currentFloor = i;
                break;
            }
        }
    }

    public void GoToFloor(string sceneRef)
    {
        if (travelling)
            return;

        travelling = true;
        Debug.Log($"We goin' to {sceneRef}");
        int nextIndex = GetIndexOfFloor(sceneRef);

        if (nextIndex == -1 || nextIndex == currentFloor)
        {
            travelling = false;
            return;
        }

        StartCoroutine(ElevatorTravel(currentFloor, nextIndex).GetEnumerator());
        shaking = true;
        StartCoroutine(ElevatorShake(transform.position, player.transform.right).GetEnumerator());
    }

    private int GetIndexOfFloor(string destinationScene)
    {
        for (int i = 0;i < levels.Length;i++)
        {
            if (levels[i].destinationScene.Equals(destinationScene))
                return i;
        }

        return -1;
    }

    private IEnumerable ElevatorTravel(int startFloor, int exitFloor)
    {
        int floorDelta = exitFloor - startFloor;
        float direction = -Mathf.Sign(floorDelta);

        while (true)
        {
            FindObjectOfType<ElevatorMenu>().SetActiveFloor(exitFloor + floorDelta);

            if (floorDelta == 0)
            {
                shaking = false;

                //Stop for a short while before exiting.
                yield return new WaitForSeconds(0.3f);
                FindObjectOfType<CORE.MapManager>().GoToNewScene(levels[exitFloor].destinationScene, levels[exitFloor].sceneTransformVariable);
                yield break; 
            }

            float timer = 0;
            while (timer < timePerFloor)
            {
                timer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            floorDelta += Mathf.FloorToInt(direction);

            //Else move the floor delta closer to zero each loop as we travel towards our destination.
            //Remember to update the ui.
        }
    }

    private IEnumerable ElevatorShake(Vector3 startPos, Vector3 playerRight)
    {
        while (shaking)
        {
            Vector3 offset = (playerRight * 0.01f * Mathf.Sin(Time.time * 40.0f));
            transform.position = startPos + offset;


            yield return new WaitForEndOfFrame();
        }

        transform.position = startPos;
    }

    [System.Serializable]
    public class FloorData
    {
        public string floorName;
        public string destinationScene;
        public string sceneTransformVariable;
        public bool realFloor = true;
    }
}
