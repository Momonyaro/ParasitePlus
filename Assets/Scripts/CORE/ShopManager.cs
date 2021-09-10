using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using MOVEMENT;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace CORE
{
    public class ShopManager : MonoBehaviour
    {
        //So what does this script do?
        // - It's responsible for handling the loading/unloading of stores, disabling the world logic while we're in
        //    one of them and enabling it once we're out. It also clears the shopping district of pedestrians for performance.
        
        
        //What's the process gonna be?
        // - Probably to start a transition, disable the world-logic and then when it's done, switch to the loading
        //    screen and stay there a short while until we finally start another transition and when that's about to be done
        //    we hide the loading screen and show the desired storefront.
        
        public List<Transition> fadeToBlacks = new List<Transition>();
        public List<Transition> fadeFromBlacks = new List<Transition>();
        
        public List<StoreEntrance> entrances = new List<StoreEntrance>();
        
        private Storefront currentStorefront = null;

        public float maxLoadingTime = 3.0f;
        public float minLoadingTime = 1.2f;
        private float timer = 0.0f;

        private void Update()
        {
            if (currentStorefront != null && currentStorefront.exitStore)
            {
                StartCoroutine(ExitStorefront());
            }
        }

        public IEnumerator EnterStorefront(Storefront storefront)
        {
            // This would start the transition to a store.
            LockWorldLogic(); // Disable player controls & the NPC crowd
            Transition fadeToBlack = fadeToBlacks[Random.Range(0, fadeToBlacks.Count)];
            while (PlayTransition(fadeToBlack).MoveNext())
            {
                //Do nothing, wait for the transition to finish
                yield return new WaitForEndOfFrame();
            }
            
            // Switch with the loading screen of the store we're loading.
            storefront.loadingScreen.SetActive(true);
            timer = Random.Range(minLoadingTime, maxLoadingTime);

            //Wait for "loading"screen to finish
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            //Swap for actual storefront (Do we want a transition or smash cut?)
            storefront.loadingScreen.SetActive(false);
            storefront.rootObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(storefront.firstButton.gameObject);

            yield return new WaitForEndOfFrame();
            currentStorefront = storefront;
            fadeToBlack.finished = false;
            yield break;
        }

        public IEnumerator ExitStorefront()
        {
            currentStorefront.exitStore = false; // To avoid repeated calls

            //Swap to the loading screen
            currentStorefront.loadingScreen.SetActive(true);
            timer = Random.Range(minLoadingTime, maxLoadingTime);

            //Wait for "loading" screen to finish
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                
                yield return new WaitForEndOfFrame();
            }
            
            currentStorefront.rootObject.SetActive(false);
            currentStorefront.loadingScreen.SetActive(false);
            
            Transition fadeFromBlack = fadeFromBlacks[Random.Range(0, fadeFromBlacks.Count)];
            while (PlayTransition(fadeFromBlack).MoveNext())
            {
                //Do nothing, wait for the transition to finish
                yield return new WaitForEndOfFrame();
            }
            //Unlock player movement
            UnlockWorldLogic();

            yield return new WaitForEndOfFrame();
            currentStorefront = null;
            fadeFromBlack.finished = false;
            yield break;
        }

        public IEnumerator PlayTransition(Transition target)
        {
            target.rootObject.SetActive(true);

            while (!target.finished)
            {
                yield return new WaitForEndOfFrame();
            }

            target.rootObject.SetActive(false);
            yield break;
        }
        
        public void LockWorldLogic()
        {
            FindObjectOfType<CrowdManager>().lockCrowd = true;
            FindObjectOfType<PlayerMovement>().lockPlayer = true;
        }
        
        public void UnlockWorldLogic()
        {
            FindObjectOfType<CrowdManager>().lockCrowd = false;
            FindObjectOfType<PlayerMovement>().lockPlayer = false;
        }

        public void CheckToEnterStore()
        {
            if (FindObjectOfType<PlayerMovement>().lockPlayer) return;
            
            for (int i = 0; i < entrances.Count; i++)
            {
                if (entrances[i].active)
                {
                    StartCoroutine(EnterStorefront(entrances[i].storefront));
                    return;
                }
            }
        }
    }
}
