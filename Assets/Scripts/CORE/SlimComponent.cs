﻿using System;
using System.Collections.Generic;
using Items;
using Scriptables;
using UnityEngine;

namespace CORE
{
    public class SlimComponent : MonoBehaviour
    {

        #region singletonStuff
        
        static SlimComponent sInstance;

        public static SlimComponent Instance
        {
            get
            {
                if (sInstance == null)
                {
                    GameObject go = new GameObject("Slim Shuttle");
                    DontDestroyOnLoad(go);
                    sInstance = go.AddComponent<SlimComponent>();
                }

                return sInstance;
            }
        }

        #endregion

        [SerializeField] private SlimData internalSlimData = new SlimData();

        public void PopulateAndSendSlim(SlimData slimData)
        {
            sInstance.internalSlimData = slimData;
        }

        public void ReadVolatileSlim(out SlimData slimData)
        {
            slimData = sInstance.internalSlimData;
            Debug.Log("reading slimData: " + sInstance.internalSlimData);
            Destroy(sInstance.gameObject);
        }

        public string ReadNonVolatilePlayerName()
        {
            return internalSlimData.playerName;
        }

        [System.Serializable]
        public class SlimData
        {
            public string destinationScene = "";
            public string lastTransformScene = "";
            public string playerName = "Yoel";
            public EntityScriptable[] partyField = new EntityScriptable[0];
            public EntityScriptable[] enemyField = new EntityScriptable[0];
            public List<Item> inventory = new List<Item>();
            public List<string> usedGroundItems = new List<string>();
            public int wallet = 0;
            public bool ignoreTransformReadFlag = false;
            public Vector3 playerLastPos = Vector3.zero;
            public Vector3 playerLastEuler = Vector3.zero;
            public int lastDungeonIndex = 0; // Default to first piece of the dungeon
        }
    }
}
