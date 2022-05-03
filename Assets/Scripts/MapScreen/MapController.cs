using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum ControllerType
{
    KEYBOARD,
    GAMEPAD
}

public class MapController : MonoBehaviour
{
    public Image playerCursor;
    public Image map;
    public Camera mapCamera;
    public float joystickSensitivity = 20;
    [SerializeField] PlayerInput playerInput;
    private ControllerType controllerType = ControllerType.GAMEPAD;
    private string lastControlScheme = "";

    public bool blockCursorMovement = false;

    private Vector2 cursorPos = Vector2.zero;
    private Vector2 joystickDelta = Vector2.zero;
    private Vector3 mapStartPos = Vector2.zero;
    private Vector3 percentage = Vector2.zero;

    [SerializeField] float maxMapWiggleDist = 50;
    private int halfWindowWidth = 1920 / 2;
    private int halfWindowHeight = 1080 / 2;

    public Vector3 GetCursorScreenPos => cursorPos;

    private void Awake()
    {
        mapStartPos = map.rectTransform.anchoredPosition;


        halfWindowWidth = Screen.width / 2;
        halfWindowHeight = Screen.height / 2;

        cursorPos = new Vector2(halfWindowWidth, halfWindowHeight);
        playerCursor.rectTransform.position = cursorPos;
        Mouse.current.WarpCursorPosition(cursorPos);
    }

    private void Update()
    {

        RefocusMapView();

        if (blockCursorMovement)
        {
            return;
        }

        if (controllerType == ControllerType.KEYBOARD)
            MouseControl();
        else if (controllerType == ControllerType.GAMEPAD)
            JoystickControl();

        if (!lastControlScheme.Equals(playerInput.currentControlScheme.ToString()))
            UpdateControllerType();
    }


    private void MouseControl()
    {
        Cursor.visible = false;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (mousePos.magnitude < 1)
            mousePos = cursorPos;

        cursorPos = mousePos;

        playerCursor.rectTransform.position = cursorPos;
    }

    private void JoystickControl()
    {
        cursorPos += joystickDelta * joystickSensitivity * Time.deltaTime;

        playerCursor.rectTransform.position = cursorPos;
    }

    private void RefocusMapView()
    {
        Vector2 currentPos = cursorPos;
        Vector2 center = new Vector2(halfWindowWidth, halfWindowHeight);

        currentPos -= center; // Place around origo

        float xPercentage = ((currentPos.x / halfWindowWidth) + 1) * 0.5f;
        float yPercentage = ((currentPos.y / halfWindowHeight) + 1) * 0.5f;
        percentage = new Vector3(xPercentage, yPercentage);

        float xLerp = Mathf.Lerp(-maxMapWiggleDist, maxMapWiggleDist, xPercentage);
        float yLerp = Mathf.Lerp(-maxMapWiggleDist, maxMapWiggleDist, yPercentage);

        Vector3 offset = new Vector2(xLerp, yLerp);

        map.rectTransform.anchoredPosition = mapStartPos - offset;
    }

    public void RetrieveJoystick(InputAction.CallbackContext context)
    {
        joystickDelta = context.ReadValue<Vector2>();
    }

    private void UpdateControllerType()
    {
        string newControls = playerInput.currentControlScheme.ToString();

        switch(newControls)
        {
            case "Keyboard&Mouse":
                controllerType = ControllerType.KEYBOARD;
                break;
            case "Gamepad":
                controllerType= ControllerType.GAMEPAD;
                break;
        }

        lastControlScheme = newControls;
    }
}
