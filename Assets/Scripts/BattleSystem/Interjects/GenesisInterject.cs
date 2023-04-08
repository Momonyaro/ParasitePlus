using SAMSARA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.Interjects
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "GenesisInterject", menuName = "Interjects/Genesis", order = 1)]
    public class GenesisInterject : InterjectBase
    {
        //This interject will basically look as though the angel is using a move but then it spawns a effect that transitions into the final dialog scene.
        public GameObject transitionPrefab;
        public Transform transitionParent;
        public float timerLength = 2f;

        private GameObject transitionInstance;
        private float _timer = 0;

        public override void Init()
        {
            transitionParent = GameObject.FindGameObjectWithTag("GENESIS_POINT").transform;
            transitionInstance = Instantiate(transitionPrefab, transitionParent);

            Samsara.Instance.PlaySFXLayered("_genesis", out bool success);

            _timer = timerLength;

            initialized = true;
        }

        public override void UpdateState(out bool endOfLife)
        {
            endOfLife = _timer <= 0;
            _timer -= Time.deltaTime;
        }

        public override void Disconnect()
        {
            initialized = false;

            //Move to dialogue scene
            battleCore.GoToDestinationScene();
        }
    }

}