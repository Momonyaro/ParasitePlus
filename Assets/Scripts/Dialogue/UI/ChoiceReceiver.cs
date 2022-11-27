using Dialogue;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoiceReceiver : MonoBehaviour
{
    public GameObject choicePrefab;
    public Transform choiceParent;
    public List<GameObject> choiceList;
    public int currentIndex;
    public int choiceCount;

    public void CreateList(ChoiceComponent.ChoiceData[] choices)
    {
        choiceList.ForEach((choice) => { choice.SetActive(false); });
        currentIndex = 0;
        choiceCount = choices.Length;
        int i = 0;

        foreach (var choice in choices)
        {
            choiceList[i].SetActive(true);
            choiceList[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = choice.text;
            choiceList[i].transform.GetChild(1).gameObject.SetActive(i == currentIndex);
            i++;
        }
    }

    public int MoveCursor(int delta)
    {
        choiceList[currentIndex].transform.GetChild(1).gameObject.SetActive(false);
        currentIndex -= delta;

        if (currentIndex < 0) { currentIndex = choiceCount - 1; }
        if (currentIndex >= choiceCount) { currentIndex = 0; }

        choiceList[currentIndex].transform.GetChild(1).gameObject.SetActive(true);

        return currentIndex;
    }
}
