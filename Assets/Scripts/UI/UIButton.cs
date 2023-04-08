using CORE;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

namespace UI
{
    public class UIButton : MonoBehaviour
    {
        public GameObject onHoverObject;
        public Selectable selectable;
        public Image background;
        public bool hovering = false;
        public bool active = true;
        public bool sendViaManager = false;
        public string msg = "";
        public UnityEvent<string> onClick = new UnityEvent<string>();
        public UnityEvent onHover = new UnityEvent();
        public UnityEvent onHoverEnd = new UnityEvent();

        private MapController mController;

        private void Awake()
        {
            mController = FindObjectOfType<MapController>();
        }

        private void Start()
        {
            if (onHoverObject != null)
                onHoverObject.SetActive(false);

            if (selectable == null)
                selectable = GetComponent<Selectable>();
        }

        public void Update()
        {
            bool currentlySelected = (EventSystem.current.currentSelectedGameObject == selectable.gameObject);

            if (!active) return;
            if (onHoverObject != null)
                onHoverObject.SetActive(currentlySelected || hovering); 
            
            if (!active) return;

            bool inside = IsInsideRect(mController.GetCursorScreenPos);

            if (!hovering && inside)
            {
                OnCursorEnter();
            }
            else if (hovering && !inside)
                OnCursorExit();
        }

        public void SendMessage()
        {
            UIManager.Instance.onUIMessage.Invoke(msg);
        }

        public void OnCursorClick()
        {
            if (!active && !hovering) return;
            SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_submit", out bool success);
            if (sendViaManager)
                UIManager.Instance.onUIMessage.Invoke(msg);
            onClick.Invoke(msg);
        }

        public void OnCursorEnter()
        {
            hovering = true;
        }

        public void OnCursorExit()
        {
            hovering = false;
        }


        public bool IsInsideRect(Vector2 pos)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(background.rectTransform, pos);
        }
    }
}
