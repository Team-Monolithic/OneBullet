using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(fileName = "TilemapSO", menuName = "LevelBuilding/Create TilemapSO")]
public class TilemapSO : ScriptableObject
{
    // [SerializeField] private PlaceType placeType; // 설치 타입
    [SerializeField] private int sortingOrder = 0; // 타일맵끼리의 z-index를 설정
    [SerializeField] private bool simulatePhysics = false; // 해당 타일맵의 타일들이 충돌을 지원할 것인지
    
    public int SortingOrder => sortingOrder;
    public bool SimulatePhysics => simulatePhysics;
}
