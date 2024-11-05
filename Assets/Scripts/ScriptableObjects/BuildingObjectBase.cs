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

[CreateAssetMenu (fileName = "Buildable", menuName = "BuildingObjects/Create Buildable")]
public class BuildingObjectBase : ScriptableObject
{
    [SerializeField] private TilemapSO tilemap; // 어떤 건설타입 및 타일맵을 사용하는지
    [SerializeField] private TileUICategorySO tileCategory; // 어떤 UI카테고리에 속하는지
    [SerializeField] private TileBase tileBase; // 실질적으로 배치될 타일
    [SerializeField] private PlaceType placeType; // 설치 타입
    [SerializeField] private string displayName; // 화면에 보일 이름
    [SerializeField] public string tag;
    public List<Event> events = new List<Event>();
    public List<ActionSO> interactionActions = new List<ActionSO>();
    
    // Public Getter
    public TilemapSO Tilemap => tilemap;
    public TileBase TileBase => tileBase;
    public TileUICategorySO TileCategory => tileCategory;
    public PlaceType PlaceType => placeType;
    public string DisplayName => displayName;
}
