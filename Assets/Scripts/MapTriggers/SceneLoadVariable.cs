using UnityEngine;

namespace MapTriggers
{
    [System.Serializable]
    public class SceneLoadVariable
    {
        public string reference = "";
        public Vector3 playerPosition = Vector3.zero;
        public Vector3 playerRotation = Vector3.zero;
    }
}