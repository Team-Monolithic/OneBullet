using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interact", menuName = "Action/Interact Action")]
public class InteractActionSO : ActionSO
{
    public override void Execute(in Dictionary<string, ActionProperty> properties, ref BuildingTile ownerObject)
    {
        if (ownerObject.baseSO is BuildingObjectInteractableSO interactableSO)
        {
            interactableSO.toggleState(ref ownerObject);
        }
    }
}