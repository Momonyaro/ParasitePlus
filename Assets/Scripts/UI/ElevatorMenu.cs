using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class ElevatorMenu : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public GameObject cursor;
    public RectTransform listTransform;
    public List<RectTransform> floorButtons = new List<RectTransform>();
    private ElevatorNode lastNode;
    public InputSystemUIInputModule uiInput;

    public GameObject realFloor;
    public GameObject falseFloor;
    private bool dumbGate;
    private bool visible;

    private void Awake()
    {
        uiInput = FindObjectOfType<InputSystemUIInputModule>();
    }

    private void OnEnable()
    {
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed += OnMouseSubmit;
        ui.FindAction("Submit").started += OnSubmit;
    }

    private void OnDisable()
    {
        var ui = uiInput.actionsAsset.FindActionMap("UI");
        ui.FindAction("Click").performed -= OnMouseSubmit;
        ui.FindAction("Submit").started -= OnSubmit;
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

        for (int i = 0; i < floorButtons.Count; i++)
        {
            ItemBtn btn = floorButtons[i].GetComponent<ItemBtn>();

            if (btn != null && btn.hovering)
            {
                btn.OnCursorClick();
            }
        }
    }

    public void CreateMenu(ElevatorNode node)
    {
        lastNode = node;

        FindObjectOfType<MapController>().blockCursorMovement = false;
        cursor.SetActive(true);
        canvasGroup.alpha = 1;

        ClearFloors();
        CreateFloors(lastNode.levels);
        visible = true;
    }

    public void HideMenu()
    {
        visible = false;
        canvasGroup.alpha = 0;
    }

    private void CreateFloors(ElevatorNode.FloorData[] floors)
    {
        for (int i = 0; i < floors.Length; i++)
        {
            GameObject button = Instantiate((floors[i].realFloor) ? realFloor : falseFloor, listTransform);

            if (floors[i].realFloor)
            {
                ItemBtn btn = button.GetComponent<ItemBtn>();
                btn.SetButtonData(floors[i].floorName, "", floors[i].destinationScene, "");
                btn.onPress.AddListener(lastNode.GoToFloor);
                btn.active = true;
            }

            floorButtons.Add(button.GetComponent<RectTransform>());
        }
    }

    public void SetActiveFloor(int index)
    {
        for (int i = 0; i < floorButtons.Count; i++)
        {
            bool active = (i == index);
            floorButtons[i].GetChild(0).GetComponent<TextAnimator>().blinkingText = active;
            if (!active)
                floorButtons[i].GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
        }
    }

    private void ClearFloors()
    {
        foreach (var floor in floorButtons)
        {
            Destroy(floor.gameObject);
        }
    }
}
