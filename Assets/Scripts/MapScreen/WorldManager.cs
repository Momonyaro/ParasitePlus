using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WorldManager : MonoBehaviour
{
    //Hold all the buttons and check for input, either mouse clicks, space bar or the a button.

    private WorldNode[] worldNodes = new WorldNode[0];
    public UI.FadeToBlackImage transition;
    [SerializeField] NodeMessage[] nodeMessages = new NodeMessage[0];
    public bool saveOnAwake = true;
    private bool dumbGate = false;

    public static int CurrentSelectionLayer = 0;

    private void Awake()
    {
        worldNodes = FindObjectsOfType<WorldNode>(true);

        CurrentSelectionLayer = CORE.SlimComponent.Instance.ReadNonVolatileBtnLayer;

        if (saveOnAwake)
        {
            CORE.SlimComponent.Instance.ReadVolatileSlim(out var saveData);

            //Save to external file
            SaveUtility.WriteToDisk(saveData);

            CORE.SlimComponent.Instance.PopulateAndSendSlim(saveData);
        }

        Debug.Log(CurrentSelectionLayer);
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

            case NodeMessage.MessageType.FADE_TO_SCENE:
                GotoScene(node.stringValue, 0.5f, Color.white);
                break;

            case NodeMessage.MessageType.GOTO_SLIM_SCENE:
                GotoScene(CORE.SlimComponent.Instance.ReadNonVolatileDesination);
                break;

            case NodeMessage.MessageType.LOAD_SAVE:
                CORE.SlimComponent.SlimData loaded = SaveUtility.ReadFromDisk(out bool success);
                if (!success) // Go to other scene
                {
                    GotoScene(node.stringValue);
                    break;
                }

                CORE.SlimComponent.Instance.PopulateAndSendSlim(loaded);
                GotoScene(loaded.destinationScene);
                break;

            case NodeMessage.MessageType.FADE_LOAD_SCENE:
                CORE.SlimComponent.SlimData loaded2 = SaveUtility.ReadFromDisk(out bool success2);
                if (!success2) // Go to other scene
                {
                    GotoScene(node.stringValue);
                    break;
                }

                CORE.SlimComponent.Instance.PopulateAndSendSlim(loaded2);
                GotoScene(loaded2.destinationScene, 0.5f, fadeTo: Color.black);
                break;

            case NodeMessage.MessageType.EXIT_APP:
                Application.Quit();
#if UNITY_EDITOR

                Debug.Log($"Quitter! :>: {node.id}");
                if (Application.isEditor)
                {
                    UnityEditor.EditorApplication.ExitPlaymode();
                }
#endif
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

    private void GotoScene(string sceneValue, float timer, Color fadeTo)
    {
        for (int i = 0; i < worldNodes.Length; i++)
        {
            worldNodes[i].hidden = true;
        }

        StartCoroutine(GotoSceneEnumerator(sceneValue, timer, fadeTo).GetEnumerator());
    }

    private IEnumerable GotoSceneEnumerator(string sceneValue, float timer, Color fadeTo)
    {
        transition.color = fadeTo;
        transition.FadeToBlack(timer - 0.1f, 0.1f);
        yield return new WaitForSeconds(timer);
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
            if (worldNodes[i].IsSelected && worldNodes[i].IsInRange(WorldManager.CurrentSelectionLayer, worldNodes[i].MinButtonLayer, worldNodes[i].MaxButtonLayer))
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
            FADE_TO_SCENE,
            EXIT_APP,
            GOTO_SLIM_SCENE,
            LOAD_SAVE,
            FADE_LOAD_SCENE,
        };

        public string id;
        public MessageType type;
        public string stringValue;
        public int intValue;
        public GameObject targetObject;
    }
}
