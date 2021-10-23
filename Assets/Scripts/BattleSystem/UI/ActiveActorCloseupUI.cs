using System;
using Scriptables;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class ActiveActorCloseupUI : MonoBehaviour
    {
        public Image image;
        public BattleSystemCore core;
        public Sprite playerSprite;
        public Sprite gummoSprite;
        public Sprite sandraSprite;
        public Sprite siveSprite;

        private void Update()
        {
            EntityScriptable current = core.GetNextEntity();
            string currentId = current.entityId;
            image.enabled = true;

            if (currentId.Equals("_player"))
                image.sprite = playerSprite;
            else if (currentId.Equals("_gummo"))
                image.sprite = gummoSprite;
            else if (currentId.Equals("_sandra"))
                image.sprite = sandraSprite;
            else if (currentId.Equals("_sive"))
                image.sprite = siveSprite;
            else
                image.enabled = false;
        }
    }
}
