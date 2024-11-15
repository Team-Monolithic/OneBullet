using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FireTrigger", menuName = "Action/FireTrigger Action")]
public class FireTriggerActionSO : ActionSO
{
    public override void Execute(in Dictionary<string, ActionProperty> properties, ref BuildingTile ownerObject)
    {
        Debug.Log("FireTrigger Action Executed");
    }
}