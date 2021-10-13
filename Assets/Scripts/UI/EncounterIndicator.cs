using System;
using CORE;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EncounterIndicator : MonoBehaviour
    {
        [Range(.5f, 5f)]public float sinScale = 1.0f;
        public Color[] fullStrColors = new Color[0];
        public Color[] shadeStrColors = new Color[0];
        public Color[] disabledColors = new Color[0];
        public Image indicator;

        private DungeonManager dm;
        
        private void Awake()
        {
            dm = FindObjectOfType<DungeonManager>();
        }

        private void Update()
        {
            UpdateIndicator(dm.encounterProgress, dm.randomEncounters);
        }

        private void UpdateIndicator(float encounterProgress, bool randomEncounters)
        {
            float sin = (Mathf.Sin(Time.time * sinScale) * 0.5f) + 0.5f;
            
            if (!randomEncounters)
            {
                indicator.color = Color.Lerp(disabledColors[0], disabledColors[1], sin);
            }
            else
            {
                Color full = Color.Lerp(fullStrColors[0], fullStrColors[1], encounterProgress);
                Color dim = Color.Lerp(shadeStrColors[0], shadeStrColors[1], encounterProgress);

                Color final = Color.Lerp(dim, full, sin);
                indicator.color = final;
            }
        }
    }
}
