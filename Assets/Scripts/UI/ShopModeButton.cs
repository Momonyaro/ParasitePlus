using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShopModeButton : MonoBehaviour
{
    public bool active = false;
    public TMPro.TextMeshProUGUI buttonText;
    public Gradient textColorGradient;
    public UnityEvent onClicked = new UnityEvent();

    private bool hovering = false;
    private RectTransform rectTransform;
    private MapController mController;

    public bool IsHovering => hovering;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        mController = FindObjectOfType<MapController>();
    }

    private void Update()
    {
        if (!active) return;

        hovering = IsCursorHovering(mController.GetCursorScreenPos);
        buttonText.color = (hovering ? textColorGradient.Evaluate(1) : textColorGradient.Evaluate(0));
    }

    public void OnCursorClick()
    {
        Debug.Log("Bruh " + gameObject.name);
        onClicked.Invoke();
    }

    bool IsCursorHovering(Vector2 cursorPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, cursorPos);
    }
}
