using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Action/Move Action")]
public class MoveActionSO : ActionSO
{
    public override void Execute(in Dictionary<string, ActionProperty> properties, ref BuildingTile ownerObject)
    {
        float X = (float)properties['X'.ToString()].value;
        float Y = (float)properties['Y'.ToString()].value;
        Debug.Log("Move Action Executed : " + X + ", " + Y);
    }
}