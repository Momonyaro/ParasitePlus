using CORE;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public GameObject onHoverObject;
        public Selectable selectable;
        public bool hovering = false;
        public bool active = true;
        public string msg = "";
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onHover = new UnityEvent();
        public UnityEvent onHoverEnd = new UnityEvent();

        private void Start()
        {
            onHoverObject.SetActive(false);

            if (selectable == null)
                selectable = GetComponent<Selectable>();
        }

        public void Update()
        {
            bool currentlySelected = (EventSystem.current.currentSelectedGameObject == selectable.gameObject);

            if (!active) return;
            onHoverObject.SetActive(currentlySelected || hovering);
        }

        public void SendMessage()
        {
            UIManager.Instance.onUIMessage.Invoke(msg);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!active) return;
            hovering = true;
            onHover.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!active) return;
            hovering = false;
            onHoverEnd.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!active) return;
            //Here we send the message to the UI manager.
            onClick.Invoke();
        }
    }
}
