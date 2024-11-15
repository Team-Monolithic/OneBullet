using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingTile
{
    public BuildingObjectBaseSO baseSO;
    private bool currentState = false;
    public Vector3Int position;
    public List<Event> events = new List<Event>();
    
    public BuildingTile(BuildingObjectBaseSO bBase, Vector3Int gridPosition)
    {
        baseSO = bBase;
        position = gridPosition;
    }
}
