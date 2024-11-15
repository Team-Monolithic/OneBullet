using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu (fileName = "Buildable", menuName = "BuildingObjects/Create Interactable")]
public class BuildingObjectInteractableSO : BuildingObjectBaseSO
{
    [SerializeField] private TileBase interactedTileBase;
    private bool currentState = false;

    public void toggleState(ref BuildingTile tile)
    {
        currentState = !currentState;
        BuildingManager.GetInstance().SetTileBase(tile, currentState ? interactedTileBase : TileBase);
    }
}
