using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "Category", menuName = "LevelBuilding/Create Category")]
public class BuildingCategory : ScriptableObject
{
    // [SerializeField] private PlaceType placeType; // 설치 타입
    [SerializeField] private int sortingOrder = 0; // 타일맵끼리의 z-index를 설정
    private Tilemap _tilemap; // 해당 카테고리의 오브젝트들이 배치될 타일맵

    public int SortingOrder => sortingOrder;

    public Tilemap Tilemap
    {
        get => _tilemap;
        set => _tilemap = value;
    }
}
