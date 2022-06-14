using CORE;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public GameObject onHoverObject;
        public Selectable selectable;
        public bool hovering = false;

        private void Start()
        {
            onHoverObject.SetActive(false);

            if (selectable == null)
                selectable = GetComponent<Selectable>();
        }

        public void Update()
        {
            bool currentlySelected = (EventSystem.current.currentSelectedGameObject == selectable.gameObject);
            
            onHoverObject.SetActive(currentlySelected || hovering);
        }

        public new void SendMessage(string msg)
        {
            UIManager.Instance.onUIMessage.Invoke(msg);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovering = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Here we send the message to the UI manager.
        }
    }
}
