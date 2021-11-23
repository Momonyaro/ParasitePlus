using UnityEngine;
using UnityEngine.Events;

namespace CORE
{
    public class UIManager : MonoBehaviour
    {
        public class UIMsgEvent : UnityEvent<string>
        {
        }
        
        public static UIManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            if (onUIMessage == null)
                onUIMessage = new UIMsgEvent();
        }

        public UIMsgEvent onUIMessage;
    }
}