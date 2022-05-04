using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldManager : MonoBehaviour
{
    //Hold all the buttons and check for input, either mouse clicks, space bar or the a button.

    private WorldNode[] worldNodes = new WorldNode[0];
    private bool dumbGate = false;

    private void Awake()
    {
        CORE.UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
    }

    private void Start()
    {
        worldNodes = FindObjectsOfType<WorldNode>();
    }

    public void ListenForMessage(string msg)
    {
        switch(msg)
        {
            case "_newGame":
                UnityEngine.SceneManagement.SceneManager.LoadScene("Intro");
                break;

            case "_quitGame":
                Application.Quit();
                if (Application.isEditor)
                {
                    UnityEditor.EditorApplication.ExitPlaymode();
                }
                break;
        }
    }



    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        SubmitAction();
    }

    public void OnMouseSubmit(InputAction.CallbackContext context)
    {
        if (dumbGate)
        {
            dumbGate = false;
            return;
        }

        if (context.performed)
        {
            dumbGate = true;
        }

        SubmitAction();
    }

    public void SubmitAction()
    {
        for (int i = 0; i < worldNodes.Length; i++)
        {
            if (worldNodes[i].IsSelected)
            {
                worldNodes[i].ProcessBtnPress();
            }
        }
    }
}
