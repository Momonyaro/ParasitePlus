using CORE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyPartyTrigger : MonoBehaviour
{

    public bool hasPlayer = true;
    public bool hasGummo = false;
    public bool hasSandra = false;
    public bool hasSive = false;

    public void TriggerEvent()
    {
        MapManager mapManager = FindObjectOfType<MapManager>();

        mapManager.OverwritePartyState(new bool[] { hasPlayer, hasGummo, hasSandra, hasSive });
    }
}
