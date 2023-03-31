using CORE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PillarPuzzle : MonoBehaviour
{
    public string id;
    public string[] solution = new string[0];
    public List<string> playerAnswer = new List<string>();
    public UnityEvent onSolved = new UnityEvent();

    private bool active = true;
    private MapManager mapManager;

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        if (mapManager.GetPersistantState(id))
        {
            active = false;
        }
    }

    public void TryKeyword(string keyword)
    {
        if (!active) return;

        //compare to solution
        int index = playerAnswer.Count;

        if (solution[index].Equals(keyword))
        {
            playerAnswer.Add(keyword);
            //check if puzzle complete
            if (IsPuzzleComplete())
            {
                //Send event that we're done and save our persistant state.
                mapManager.WritePersistantState(id, true);
                onSolved.Invoke();
                active = false;
            }

            return;
        }

        //else input is wrong, clear the list
        playerAnswer.Clear();
    }

    private bool IsPuzzleComplete()
    {
        return solution.Length == playerAnswer.Count;
    }
}
