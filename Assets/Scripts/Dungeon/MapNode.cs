using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dungeon
{
    public class MapNode : MonoBehaviour
    {
        //Find nearby MapNodes, create bridges between them to allow the player to walk between them.
        public Transform nodeFocus;

        private void OnDrawGizmos()
        {
            Vector3 focusPos = nodeFocus.position;
            
            //Gizmos.DrawIcon(focusPos, "AvatarSelector@2x", true);
            
            Vector3 sample = Vector3.forward;
            RaycastHit hit;
            for (int i = 0; i < 4; i++)
            {
                Physics.Raycast(focusPos, sample, out hit, 1.5f);
                if (hit.collider == null || hit.collider.isTrigger)
                {
                    Gizmos.DrawLine(focusPos, focusPos + sample * 0.5f);
                }

                sample = Quaternion.Euler(0, 90, 0) * sample;
            }
        }
    }
}
