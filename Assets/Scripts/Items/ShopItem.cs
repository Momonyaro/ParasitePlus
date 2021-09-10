using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Items
{
    public class ShopItem : MonoBehaviour
    {
        public bool selected = false;
        public int buyAmount = 0; // How many of the item we buy when confirming
        public Item item = null;

        private const float InactiveWidth = 1948.346f;
        [SerializeField] private Color inactiveColor = new Color(); 
        private const float ActiveWidth = 2012.524f;
        [SerializeField] private Color activeColor = new Color();

        [SerializeField] private Image image;
        [SerializeField] private GameObject activeUI;
        [SerializeField] private TextMeshProUGUI itemTitle;
        [SerializeField] private TextMeshProUGUI itemPrice;
        [SerializeField] private TextMeshProUGUI itemBuyAmount;

        private RectTransform rectTransform;
        [SerializeField] private InputActionAsset module;
        private InputActionMap inputActionMap;
        private InputAction iterateCount;
        private ShopInterface parent;

        //Also store a private reference to the parent later

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            inputActionMap = module.FindActionMap("UI");
            iterateCount = inputActionMap.FindAction("Move");
            iterateCount.started += SampleInputModule;
        }

        private void Update()
        {
            selected = (EventSystem.current.currentSelectedGameObject == gameObject);

            var old = rectTransform.sizeDelta;
            
            if (selected)
            {
                image.color = activeColor;
                if (!activeUI.activeInHierarchy)
                {
                    activeUI.SetActive(true);
                }
                rectTransform.sizeDelta = new Vector2(ActiveWidth, old.y);
                parent.lastSelected = this;
            }
            else
            {
                image.color = inactiveColor;
                if (activeUI.activeInHierarchy)
                {
                    activeUI.SetActive(false);
                }
                rectTransform.sizeDelta = new Vector2(InactiveWidth, old.y);
            }
        }

        public void InitStoreItem(Item toStore, ShopInterface shopParent)
        {
            item = toStore;
            buyAmount = 0;

            this.parent = shopParent;
            
            UpdateText();
        }

        private void SampleInputModule(InputAction.CallbackContext obj)
        {
            if (selected)
            {
                Vector2 readFromEvent = obj.ReadValue<Vector2>();
                float x = readFromEvent.x;
                if (x > 0.2f)
                {
                    AddToBuyAmount(1);
                }
                else if (x < -0.2f)
                {
                    AddToBuyAmount(-1);
                }
                
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        private void OnDestroy()
        {
            iterateCount.started -= SampleInputModule;
        }

        private void UpdateText()
        {
            itemTitle.text = item.name;
            itemPrice.text = (buyAmount > 0) ? buyAmount + " x " + item.msrp + " kr" : item.msrp + " kr";
            itemBuyAmount.text = buyAmount.ToString();
        }

        public void AddToBuyAmount(int value)
        {
            buyAmount += value;
            buyAmount = Mathf.Clamp(buyAmount, 0, 99);
            UpdateText();
        }
        
    }
}
