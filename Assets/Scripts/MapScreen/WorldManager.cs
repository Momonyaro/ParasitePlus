using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldManager : MonoBehaviour
{
    //Hold all the buttons and check for input, either mouse clicks, space bar or the a button.

    private WorldNode[] worldNodes = new WorldNode[0];
    [SerializeField] NodeMessage[] nodeMessages = new NodeMessage[0];
    private bool dumbGate = false;

    public static int CurrentSelectionLayer = 0;

    private void Start()
    {
        worldNodes = FindObjectsOfType<WorldNode>(true);
        WorldManager.CurrentSelectionLayer = 0;
    }

    private void OnEnable()
    {
        CORE.UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
    }

    private void OnDisable()
    {
        CORE.UIManager.Instance.onUIMessage.RemoveListener(ListenForMessage);
    }

    public void ListenForMessage(string msg)
    {
        for (int i = 0; i < nodeMessages.Length; i++)
        {
            if (nodeMessages[i].id.Equals(msg))
            {
                HandleMessage(nodeMessages[i]);
            }
        }
    }

    private void HandleMessage(NodeMessage node)
    {
        switch(node.type)
        {
            case NodeMessage.MessageType.OPEN_SUBMENU:
                ModifySubmenu(node, true);
                break;

            case NodeMessage.MessageType.CLOSE_SUBMENU:
                ModifySubmenu(node, false);
                break;

            case NodeMessage.MessageType.GOTO_SCENE:
                GotoScene(node.stringValue);
                break;

            case NodeMessage.MessageType.GOTO_SLIM_SCENE:
                GotoScene(CORE.SlimComponent.Instance.ReadNonVolatileDesination);
                break;

            case NodeMessage.MessageType.EXIT_APP:
                Application.Quit();
                if (Application.isEditor)
                {
                    UnityEditor.EditorApplication.ExitPlaymode();
                }
                break;
        }
    }

    private void ModifySubmenu(NodeMessage node, bool visibility)
    {
        WorldSubmenu submenu = node.targetObject.GetComponent<WorldSubmenu>();
        CurrentSelectionLayer = node.intValue;
        submenu.SetVisibility(visibility);
    }

    // Make it fade to black first
    private void GotoScene(string sceneValue)
    {
        SceneParser.ParseSceneChange(sceneValue, out string slimDestination, out string destination);

        CORE.SlimComponent.Instance.SetNonVolatileDestination(slimDestination);
        UnityEngine.SceneManagement.SceneManager.LoadScene(destination);
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
            if (worldNodes[i].IsSelected && worldNodes[i].ButtonLayer == WorldManager.CurrentSelectionLayer)
            {
                worldNodes[i].ProcessBtnPress();
            }
        }
    }

    [System.Serializable]
    internal class NodeMessage
    {
        internal enum MessageType
        {
            OPEN_SUBMENU,
            CLOSE_SUBMENU,
            GOTO_SCENE,
            EXIT_APP,
            GOTO_SLIM_SCENE,
        };

        public string id;
        public MessageType type;
        public string stringValue;
        public int intValue;
        public GameObject targetObject;
    }
}
