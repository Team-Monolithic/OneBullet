using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 설치 타입에 따라 (단일, 라인, 사각형)
public enum PlaceType
{
    None,
    Single,
    Line,
    Rectangle
}

public enum EventType
{
    Started
}

[CreateAssetMenu (fileName = "Buildable", menuName = "BuildingObjects/Create Buildable")]
public class BuildingObjectBase : ScriptableObject
{
    [SerializeField] private BuildingCategory buildingCategory; // 어떤 건설타입 및 타일맵을 사용하는지
    [SerializeField] private UICategory uiCategory; // 어떤 UI카테고리에 속하는지
    [SerializeField] private TileBase tileBase; // 실질적으로 배치될 타일
    [SerializeField] private PlaceType placeType; // 설치 타입
    [SerializeField] private string displayName; // 화면에 보일 이름
    [SerializeField] public string tag;
    
    // Public Getter
    public BuildingCategory BuildingCategory => buildingCategory;
    public TileBase TileBase => tileBase;
    public UICategory UICategory => uiCategory;
    public PlaceType PlaceType => placeType;
    public string DisplayName => displayName;
}
