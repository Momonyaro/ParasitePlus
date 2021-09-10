using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CORE
{
    public class CrowdManager : MonoBehaviour
    {
        //This script manages spawning the pedestrians at the start point of a random path.
        public int maxPedestrians = 20;
        [HideInInspector] public int currentPedestrians = 0;
        public float pedMoveSpeed = 1.1f;
        public float timeBetweenSpawns = 0.3f;
        private float spawnTimer = 0;
        public float randomTimeOffset = 0.1f;
        public bool lockCrowd = false;
        
        [Space]
        public List<CrowdPath> possiblePaths = new List<CrowdPath>();
        public GameObject pedestrianPrefab = null;

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                possiblePaths.Add(transform.GetChild(i).GetComponent<CrowdPath>());
            }

            for (int i = 0; i < possiblePaths.Count; i++)
            {
                possiblePaths[i].pedMoveSpeed = pedMoveSpeed;
                possiblePaths[i].manager = this;
            }

            spawnTimer = timeBetweenSpawns + Random.Range(-randomTimeOffset, randomTimeOffset);
        }

        private void Update()
        {
            if (currentPedestrians >= maxPedestrians) return;
            if (lockCrowd)
            {
                return;
            }

            for (int i = 0; i < possiblePaths.Count; i++)
            {
                possiblePaths[i].UpdatePedestrians();
            }
            
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SelectRandomPath();
                spawnTimer = timeBetweenSpawns + Random.Range(-randomTimeOffset, randomTimeOffset);
            }
        }

        private void SelectRandomPath()
        {
            while (true)
            {
                int randomIndex = Random.Range(0, possiblePaths.Count);
                if (Random.value < possiblePaths[randomIndex].pathChance)
                {
                    HandPedestrianToPath(possiblePaths[randomIndex]);
                    return;
                }
            }
        }

        private void HandPedestrianToPath(CrowdPath path)
        {
            path.AddPedestrianToPath(pedestrianPrefab);
            currentPedestrians++;
        }
    }
}
