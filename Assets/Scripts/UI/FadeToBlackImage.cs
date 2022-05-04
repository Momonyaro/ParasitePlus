using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FadeToBlackImage : MonoBehaviour
    {
        public Image image;
        public bool finished = true;
        public bool screenBlack = false;
        public bool playOnStart = false;

        private void Start()
        {
            if (playOnStart)
            {
                FadeFromBlack(3f);
            }
        }

        public void FadeToBlack(float fadeTime, float blackScreenTime, bool cancelHalfway = false)
        {
            StartCoroutine(FadeToBlackEnumerator(fadeTime, blackScreenTime, cancelHalfway));
        }

        public void FadeFromBlack(float fadeTime)
        {
            StartCoroutine(FadeFromBlackEnumerator(fadeTime));
        }

        private IEnumerator FadeToBlackEnumerator(float fadeTime, float blackScreenTime, bool cancelHalfway = false)
        {
            if (!finished) yield break; // We're already running one of these.
            
            finished = false;
            float timePassed = 0;
            while (timePassed < fadeTime)
            {
                image.color = new Color(0, 0, 0, timePassed / fadeTime);
                timePassed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            timePassed = 1;
            image.color = new Color(0, 0, 0, timePassed);

            screenBlack = true;
            yield return new WaitForSeconds(blackScreenTime);
            screenBlack = false;

            if (cancelHalfway)
            {
                finished = true;
                
                yield break;
            }
            
            while (timePassed > 0)
            {
                image.color = new Color(0, 0, 0, timePassed / fadeTime);
                timePassed -= Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }
            
            image.color = new Color(0, 0, 0, 0);

            finished = true;
            yield break;
        }

        private IEnumerator FadeFromBlackEnumerator(float fadeTime)
        {
            if (!finished) yield break; // We're already running one of these.

            finished = false;
            float timePassed = 0;
            while (timePassed < fadeTime)
            {
                image.color = new Color(0, 0, 0, 1 - (timePassed / fadeTime));
                timePassed += Time.deltaTime;

                yield return new WaitForEndOfFrame();
            }

            image.color = new Color(0, 0, 0, 0);

            finished = true;
            yield break;
        }
    }
}
