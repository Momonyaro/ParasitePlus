using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapTriggers
{
    public class LampPulser : MonoBehaviour
    {
        public List<Light> lights = new List<Light>();
        public Color highCol = Color.white;
        public Color lowCol = Color.black;
        public AnimationCurve lerpCurve = new AnimationCurve();
        public float currentTime = 0;
        public int sign = 1;

        public bool running = false;
        public bool midLerp = false;

        public void StartPulser()
        {
            running = true;
        }
        
        private void Update()
        {
            if (!running && !midLerp) return;
            
            float maxTime = lerpCurve.keys[lerpCurve.length - 1].time;
            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].color = Color.Lerp(lowCol, highCol, lerpCurve.Evaluate(currentTime));
            }

            currentTime += Time.deltaTime * sign;

            if (currentTime < 0 || currentTime > maxTime)
            {
                sign *= -1;
                midLerp = false;
            }
            else
            {
                midLerp = true;
            }
        }
    }
}
