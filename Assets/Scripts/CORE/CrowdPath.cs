using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace CORE
{
    public class CrowdPath : MonoBehaviour
    {
        public List<Vector3> pathNodes = new List<Vector3>();
        public List<PathPedContainer> pathPeds = new List<PathPedContainer>();
        [HideInInspector] public float pedMoveSpeed = 1.0f;
        public float pathChance = 0.0f;
        public Color gizmoCol = Color.green;
        private float minNextNodeDist = 0.1f;
        [HideInInspector] public CrowdManager manager;

        public void UpdatePedestrians()
        {
            for (int i = 0; i < pathPeds.Count; i++)
            {
                if (pathPeds[i].pedestrian == null)
                {
                    pathPeds.RemoveAt(i);
                    break;
                }
                
                PathPedContainer temp = pathPeds[i];
                MovePedestrian(ref temp, i, out bool pedLeft);
                if (pedLeft)
                {
                    manager.currentPedestrians--;   
                    break;
                }
                pathPeds[i] = temp;
            }
        }

        public void AddPedestrianToPath(GameObject pedestrian)
        {
            GameObject instance = GameObject.Instantiate(pedestrian, pathNodes[0], Quaternion.LookRotation(pathNodes[1], Vector3.up), transform);
            
            PathPedContainer pedContainer = new PathPedContainer(instance, 1);
            pathPeds.Add(pedContainer);
        }

        private void MovePedestrian(ref PathPedContainer ped, int i, out bool pedLeft)
        {
            Transform pedTransform = ped.pedestrian.transform;
            
            //Check distance
            if (Vector3.Distance(pedTransform.position, pathNodes[ped.nextNode]) < minNextNodeDist)
            {
                if (ped.nextNode + 1 >= pathNodes.Count)
                {
                    Destroy(ped.pedestrian);
                    pathPeds.RemoveAt(i);
                    pedLeft = true;
                    return;
                }
                
                ped.nextNode++;
            }

            pedLeft = false;
            pedTransform.position = Vector3.MoveTowards(pedTransform.position, pathNodes[ped.nextNode], pedMoveSpeed * Time.deltaTime);
            pedTransform.LookAt(2 * pedTransform.position - pathNodes[ped.nextNode]);
        }

        public void CullAllPedestrians()
        {
            for (int i = 0; i < pathPeds.Count; i++)
            {
                Destroy(pathPeds[i].pedestrian);
            }
            pathPeds.Clear();
        }
        
        private void OnDrawGizmos()
        {
            for (int i = 1; i < pathNodes.Count; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(pathNodes[i - 1], pathNodes[i]);
                
                Vector3 normalDir = pathNodes[i] - pathNodes[i - 1];
                normalDir = normalDir.normalized;
                Vector3 normalPos = pathNodes[i] - normalDir * 0.3f;
                
                Gizmos.color = gizmoCol;
                Gizmos.DrawLine(pathNodes[i], normalPos);
                Gizmos.DrawLine(pathNodes[i], pathNodes[i] + (Quaternion.Euler(0, 30, 0) * -normalDir) * 0.15f);
                Gizmos.DrawLine(pathNodes[i], pathNodes[i] + (Quaternion.Euler(0, -30, 0) * -normalDir) * 0.15f);
            }
        }
    }

    public struct PathPedContainer
    {
        public int nextNode;
        public GameObject pedestrian { get; private set; }

        public PathPedContainer(GameObject ped, int nextNode)
        {
            this.nextNode = nextNode;
            pedestrian = ped;
        }
    }
}
