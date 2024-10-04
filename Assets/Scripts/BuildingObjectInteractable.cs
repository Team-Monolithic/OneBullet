using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Buildable", menuName = "BuildingObjects/Create Interactable")]
public class BuildingObjectInteractable : BuildingObjectBase
{
    [SerializeField] private int eventIndex;
    
    public int EventIndex => eventIndex;
}
