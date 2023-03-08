using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public AnimationCurve revealCurve;
    public AnimationCurve hideCurve;
    public Gradient backgroundGradient;
    public CanvasGroup background;
    public GameObject cursor;
    public UIButton[] partyCards;
    public InputSystemUIInputModule uiInput;


    private WorldNode[] worldNodes = new WorldNode[0];
    [SerializeField] NodeMessage[] nodeMessages = new NodeMessage[0];
    public bool visible = false;
    private bool dumbGate = false;
    MOVEMENT.FPSGridPlayer lastPlayer;

    public static int CurrentSelectionLayer = 0;
    private Coroutine transition;

    private void Awake()
    {
        worldNodes = FindObjectsOfType<WorldNode>(true);
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
    }

    private void Start()
    {
        lastPlayer = FindObjectOfType<MOVEMENT.FPSGridPlayer>();
        SetAllInvisible();
    }

    private void OnEnable()
    {
        CORE.UIManager.Instance.onUIMessage.AddListener(ListenForMessage);
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed += OnMouseSubmit;
        ui.FindAction("Submit").started += OnSubmit;
    }

    private void OnDisable()
    {
        CORE.UIManager.Instance.onUIMessage.RemoveListener(ListenForMessage);
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed -= OnMouseSubmit;
        ui.FindAction("Submit").started -= OnSubmit;
    }

    public void ListenForMessage(string msg)
    {

        if (msg.Equals("_togglePauseMenu"))
        {
            if (!visible && lastPlayer.IsLocked)
                return;
            visible = !visible;
            FindObjectOfType<MapController>().blockCursorMovement = !visible;
            if (visible) lastPlayer.AddLock("PAUSE_MENU");
            else lastPlayer.RemoveLock("PAUSE_MENU");

            SwitchTransition(IEVisible(visible).GetEnumerator());
            return;
        }

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
        switch (node.type)
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
#if UNITY_EDITOR
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
        for (int i = 0; i < worldNodes.Length; i++)
        {
            if (worldNodes[i].IsInRange(CurrentSelectionLayer, worldNodes[i].MinButtonLayer, worldNodes[i].MaxButtonLayer))
            {
                if (worldNodes[i].alwaysShowExtInfo && worldNodes[i].extInfoTab != null)
                {
                    worldNodes[i].extInfoTab.SetVisibility(true);
                }
            }
            else
            {
                if (worldNodes[i].alwaysShowExtInfo && worldNodes[i].extInfoTab != null)
                {
                    worldNodes[i].extInfoTab.SetVisibility(false);
                }
            }
        }
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
        if (!visible) return;

        for (int i = 0; i < worldNodes.Length; i++)
        {
            if (worldNodes[i].IsSelected && worldNodes[i].IsInRange(CurrentSelectionLayer, worldNodes[i].MinButtonLayer, worldNodes[i].MaxButtonLayer))
            {
                worldNodes[i].ProcessBtnPress();
            }
        }
    }

    private void SetAllInvisible()
    {
        background.alpha = 0;
    }

    private void SwitchTransition(IEnumerator IEtransition)
    {
        if (transition != null)
            StopCoroutine(transition);

        transition = StartCoroutine(IEtransition);
    }

    private IEnumerable IEVisible(bool visible)
    {
        cursor.SetActive(visible);
        AnimationCurve curve = visible ? revealCurve : hideCurve;
        partyCards.ToList().ForEach((o) => { o.active = visible; });
        if (visible) FindObjectOfType<PauseCardUpdater>().UpdateEntities();

        for (int i = 0; i < worldNodes.Length; i++)
        {
            worldNodes[i].IsSelected = false;
        }

        float timer = 0;
        float maxTime = curve.keys[curve.keys.Length - 1].time;

        while (timer < maxTime)
        {
            var aa = curve.Evaluate(timer);
            background.alpha = (aa);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        background.alpha = (curve.Evaluate(maxTime));


        for (int i = 0; i < worldNodes.Length; i++)
        {
            worldNodes[i].gameObject.SetActive(visible);
        }

        yield break;
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
