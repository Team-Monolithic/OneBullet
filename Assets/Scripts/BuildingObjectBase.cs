using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu (fileName = "Buildable", menuName = "BuildingObjects/Create Buildable")]
public class BuildingObjectBase : ScriptableObject
{
    [SerializeField] private BuildingCategory buildingCategory; // 어떤 건설타입 및 타일맵을 사용하는지
    [SerializeField] private UICategory uiCategory; // 어떤 UI카테고리에 속하는지
    [SerializeField] private TileBase tileBase; // 실질적으로 배치될 타일
    // [SerializeField] private PlaceType placeType; // category와는 다른 place type을 사용할 수 있는 가능성이 있을수도..
    
    // Public Getter
    public BuildingCategory BuildingCategory => buildingCategory;
    public TileBase TileBase => tileBase;
    public UICategory UICategory => uiCategory;
    // public PlaceType PlaceType
    // {
    //     get
    //     {
    //         return placeType == PlaceType.None ? placeType : category.PlaceType;
    //     }
    // }
    
    
}
