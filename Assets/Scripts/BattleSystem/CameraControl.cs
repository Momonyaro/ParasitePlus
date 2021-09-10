using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem
{
    public class CameraControl : MonoBehaviour
    {
        // This component will be responsible for placing effects on the camera to give it all a bit more oomph.
        
        //We will be able to lerp towards several points that are closer to certain enemies when attacking
        // to make it look as though the one attacking is stepping closer to them.
        
        //Also take this opportunity to create a system for camera shake.

        private Vector3 originalCamPos;
        public Camera mainCam;
        public List<Transform> enemyFieldCloseUpPositions = new List<Transform>();

        [Range(0, 4)]
        public int testIndex = 0;
        
        public AnimationCurve closeUpLerpCurve = new AnimationCurve();
        public AnimationCurve camShakeCurve = new AnimationCurve();

        private void Awake()
        {
            originalCamPos = mainCam.transform.position;
        }

        public void CloseUpOnEnemy(int enemyIndex, float timeScale, float holdTime)
        {
            
            /* DEBUG */
            //enemyIndex = testIndex;
            
            Vector3 endPos = enemyFieldCloseUpPositions[enemyIndex].position;
            StartCoroutine(CloseUpEnumerator(originalCamPos, endPos, timeScale, holdTime));
        }

        public void CameraShake(float timeScale, float magnitude)
        {
            Vector3 origo = mainCam.transform.position;
            StartCoroutine(CamShakeEnumerator(origo, timeScale, magnitude));
        }

        private IEnumerator CamShakeEnumerator(Vector3 origo, float timeScale, float magnitude)
        {
            float timePassed = 0;
            float lerpCurveLastX = camShakeCurve.keys[camShakeCurve.length - 1].time;
            mainCam.transform.position = origo;

            while (timePassed < lerpCurveLastX)
            {
                Vector3 shakeVec = new Vector3(camShakeCurve.Evaluate(timePassed), 
                                                camShakeCurve.Evaluate(lerpCurveLastX - timePassed), 
                                                camShakeCurve.Evaluate((timePassed + (lerpCurveLastX * 0.5f) % lerpCurveLastX)));

                var currentCamPos = mainCam.transform.position;
                Vector3 lerped = Vector3.Lerp(currentCamPos, origo + (shakeVec * magnitude), 0.5f);

                mainCam.transform.position = lerped;
                
                timePassed += Time.deltaTime * timeScale;
                timePassed = Mathf.Clamp(timePassed, 0, lerpCurveLastX);
                yield return new WaitForEndOfFrame();
            }
            
            mainCam.transform.position = origo;
            
            yield break;
        }

        private IEnumerator CloseUpEnumerator(Vector3 startPoint, Vector3 endPoint, float timeScale, float holdTime)
        {
            float timePassed = 0;
            float lerpCurveLastX = closeUpLerpCurve.keys[closeUpLerpCurve.length - 1].time;
            mainCam.transform.position = startPoint; //Reset camera position to start;
            
            while (timePassed < lerpCurveLastX)
            {
                Vector3 lerped = Vector3.Lerp(mainCam.transform.position, endPoint, closeUpLerpCurve.Evaluate(timePassed));
                mainCam.transform.position = lerped;
                
                timePassed += Time.deltaTime * timeScale;
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSecondsRealtime(holdTime);
            timePassed = 0;
            
            while (timePassed < lerpCurveLastX)
            {
                Vector3 lerped = Vector3.Lerp(mainCam.transform.position, startPoint, closeUpLerpCurve.Evaluate(timePassed));
                mainCam.transform.position = lerped;
                
                timePassed += Time.deltaTime * timeScale;
                yield return new WaitForEndOfFrame();
            }
            
            yield break;
        }
    }
}
