using UnityEngine;

namespace BattleSystem.UI
{
    public class ShadeCopier : MonoBehaviour
    {
        public SpriteRenderer toCopy;
        public new SpriteRenderer renderer; 

        // Update is called once per frame
        void Update()
        {
            renderer.color = toCopy.color;
        }
    }
}
