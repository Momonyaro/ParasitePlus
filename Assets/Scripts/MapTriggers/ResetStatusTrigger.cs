using CORE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetStatusTrigger : MonoBehaviour
{
    public void TriggerEvent()
    {
        MapManager mapManager = FindObjectOfType<MapManager>();

        Scriptables.EntityScriptable[] party = mapManager.currentSlimData.partyField;

        foreach (var member in party)
        {
            if (member == null) continue;

            Vector2Int hp = member.GetEntityHP();
            Vector2Int ap = member.GetEntityAP();

            hp.x = hp.y;
            ap.x = ap.y;

            member.SetEntityHP(hp);
            member.SetEntityAP(ap);
        }

        mapManager.currentSlimData.partyField = party;
    }
}
