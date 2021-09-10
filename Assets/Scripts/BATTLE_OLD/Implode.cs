using UnityEngine;

namespace BATTLE
{
    public class Implode : MonoBehaviour
    {
        public bool implodeSwitch = false;

        private void Start()
        {
            implodeSwitch = false;
        }

        private void Update()
        {
            if (implodeSwitch)
                ImplodeThisEntity();
        }

        public void ImplodeThisEntity()
        {
            Destroy(gameObject);
        }
    }
}
