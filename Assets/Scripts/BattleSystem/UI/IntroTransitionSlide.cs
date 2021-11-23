using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class IntroTransitionSlide : MonoBehaviour
    {
        private float transitionValue;

        private void Awake()
        {
        }

        private void Start()
        {
            StartCoroutine(IntroTransition());
        }

        IEnumerator IntroTransition()
        {
            transitionValue = 1;
            while (transitionValue > 0)
            {
                GetComponent<Image>().materialForRendering.SetFloat("_FillAmount", transitionValue);
                

                transitionValue -= Time.deltaTime * 2.0f;
                yield return new WaitForEndOfFrame();
            }

            transitionValue = 0;
            GetComponent<Image>().materialForRendering.SetFloat("_FillAmount", 0);
            
            yield break;
        }
    }
}
