using System;
using System.Collections.Generic;
using CORE;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Items
{
    public class ShopInterface : MonoBehaviour
    {
        public ItemType storeType;
        public GameObject storeItemPrefab;
        public Transform storeItemParent;
        public ScrollRect scrollView;
        public TextMeshProUGUI walletText;
        private ItemDatabase itemDatabase;
        public ShopItem lastSelected = null;
        private bool storeActive = false;
        public Storefront storefront;
        
        [SerializeField] private InputActionAsset module;
        private InputActionMap inputActionMap;
        private InputAction backOut;


        private void Awake()
        {
            inputActionMap = module.FindActionMap("Player");
            backOut = inputActionMap.FindAction("BuyMenuBack");
            backOut.started += BackOutFromMenu;
        }

        private void Start()
        {
            itemDatabase = Resources.Load<ItemDatabase>("Item Database");
        }

        public void LoadStore()
        {
            Item[] items = itemDatabase.GetAllItemsOfType(storeType, true); // Also factor in player level in the future
            SpringClean();
            UpdateWallet(0);

            storeActive = true;

            List<GameObject> spawned = new List<GameObject>(); 
            
            for (int i = 0; i < items.Length; i++)
            {
                spawned.Add(Instantiate(storeItemPrefab, storeItemParent));
                spawned[i].GetComponent<ShopItem>().InitStoreItem(items[i], this);
            }

            for (int i = 1; i < spawned.Count; i++)
            {
                Selectable last = spawned[i - 1].GetComponent<Selectable>();
                Selectable current = spawned[i].GetComponent<Selectable>();

                var lastNavigate = last.navigation;
                lastNavigate.selectOnDown = current;
                last.navigation = lastNavigate;
                
                var currentNavigate = current.navigation;
                currentNavigate.selectOnUp = last;
                current.navigation = currentNavigate;
            }
            
            EventSystem.current.SetSelectedGameObject(spawned[0]);
        }

        public void LoadSell()
        {
            SpringClean();
            UpdateWallet(500);
            
            storeActive = true;
        }

        private void UpdateWallet(int difference)
        {
            int newValue = FindObjectOfType<MapManager>().UpdateWallet(difference);
            walletText.text = newValue + " kr";
        }
        
        private void BackOutFromMenu(InputAction.CallbackContext obj)
        {
            Debug.Log("asdas");
            if (storeActive)
            {
                SpringClean();
                storefront.SetStoreState(0);
                EventSystem.current.SetSelectedGameObject(storefront.firstButton.gameObject);
                storeActive = false;
            }
        }

        public int SumCartCost(List<Item> cart)
        {
            int sum = 0;
            for (int i = 0; i < cart.Count; i++)
            {
                sum += cart[i].msrp;
            }

            return sum;
        }

        public void SpringClean()
        {
            for (int i = 0; i < storeItemParent.childCount; i++)
            {
                Destroy(storeItemParent.GetChild(i).gameObject);
            }
            lastSelected = null;
        }
    }
}
