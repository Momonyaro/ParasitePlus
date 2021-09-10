using System;
using UnityEngine;

namespace MOVEMENT
{
    public class CameraPivot : MonoBehaviour
    {
        public float minRange = 0.1f;
        public float maxRange = 2.5f;

        public Transform closeUp;
        public Transform farAway;

        public Transform pivotCam;
        
        private void FixedUpdate()
        {
            // Fetch current distance to closest wall
            float distance = GetDistanceToWall();
            float clampedDist = Mathf.Clamp(distance, minRange, maxRange);

            //Get both to start at 0
            float shortMaxRange = maxRange - minRange;
            float shortDist = clampedDist - minRange;

            float percentage = shortDist / shortMaxRange; // Will give percentage distance between closeUp and farAway
            
            pivotCam.position = Vector3.Lerp(closeUp.position, farAway.position, percentage);
        }

        /// <summary>
        /// Shoot raycast from closeUp to farAway. Check the distance to wall
        /// </summary>
        /// <returns>Returns distance as float</returns>
        private float GetDistanceToWall()
        {
            Vector3 closeUpPos = closeUp.position;
            Vector3 farAwayPos = farAway.position;

            RaycastHit[] hits = Physics.RaycastAll(closeUpPos, farAwayPos - closeUpPos, 10.0f);

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.isTrigger) { continue; }
                
                if (!hits[i].collider.CompareTag("Player"))
                {
                    //It's not the player, we're interested in this item.
                    return hits[i].distance;
                }
            }

            return maxRange;
        }

        private void OnDrawGizmos()
        {
            Vector3 closeUpPos = closeUp.position;
            Vector3 farAwayPos = farAway.position;
            
            Gizmos.DrawRay(closeUpPos, (farAwayPos - closeUpPos) * maxRange);
        }
    }
}
