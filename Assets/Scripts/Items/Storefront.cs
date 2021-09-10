using System;
using UnityEngine;
using UnityEngine.UI;

namespace Items
{
    public class Storefront : MonoBehaviour
    {
        public GameObject loadingScreen;
        public GameObject rootObject;
        public Animator uiAnimator;
        public Button firstButton;
        public bool exitStore = false;

        public void SetStoreState(int newState)
        {
            uiAnimator.SetInteger("StoreState", newState);
        }
    }
}
