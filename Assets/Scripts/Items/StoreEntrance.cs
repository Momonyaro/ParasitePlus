using System;
using MOVEMENT;
using UnityEngine;

namespace Items
{
    public class StoreEntrance : MonoBehaviour
    {
        public Animator entranceAnimator;
        public Storefront storefront;
        public bool active = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerMovement>())
            {
                entranceAnimator.SetBool("ShowMenu", true);
                active = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerMovement>())
            {
                entranceAnimator.SetBool("ShowMenu", false);
                active = false;
            }
        }
    }
}
