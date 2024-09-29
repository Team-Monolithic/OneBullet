using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class SaveHandler : MonoBehaviour
{
    private Dictionary<string, Tilemap> tilemaps = new Dictionary<string, Tilemap>(); // 맵 이름과 맵 자체를 매핑. 맵 이름은 unique해야 함
    [SerializeField] private string fileName = "tilemapData.json";

    private void Start() // BuildingCreator.Start() 보다 후에 실행되어야 함
    {
        InitializeMaps();
    }

    private void InitializeMaps()
    {
        Tilemap[] maps = FindObjectsOfType<Tilemap>();

        foreach (Tilemap map in maps)
        {
            if (map.name == "Preview") // 프리뷰 맵은 저장하지 않음
            {
                continue;
            }
            tilemaps.Add(map.name, map);
        }
    }

    public void OnSave()
    {
        List<TilemapData> tilemapDatas = new List<TilemapData>(); // 실질적으로 저장할 데이터 리스트

        foreach (var mapObject in tilemaps)
        {
            // mapObject: 저장할 여러 개의 타일맵 중, 현재 iteration에서 탐색하고 있는 타일맵
            string mapName = mapObject.Key;
            Tilemap currentMap = mapObject.Value;
            
            TilemapData currentMapData = new TilemapData(); // string mapName, List<TileInfo> tiles
            currentMapData.mapName = mapName;

            // 범위를 순회하면서 currentMapData.tiles를 채운다.
            // cellBounds = 타일맵에 존재하는 타일들의 좌표 범위
            // [xMin, xMax) 형태의 구간
            BoundsInt boundsForThisMap = currentMap.cellBounds;
            for (int x = boundsForThisMap.xMin; x < boundsForThisMap.xMax; x++)
            {
                for (int y = boundsForThisMap.yMin; y < boundsForThisMap.yMax; y++)
                {
                    Vector3Int currentPos = new Vector3Int(x, y, 0);
                    TileBase tile = currentMap.GetTile(currentPos);
                    if (tile is null)
                    {
                        continue;
                    }
                    
                    // 타일의 guid와 로컬 식별자를 가져옴
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tile, out string guid, out long localId))
                    {
                        TileInfo tileInfo = new TileInfo(tile, currentPos, guid);
                        currentMapData.tiles.Add(tileInfo); // currentMapData.tiles에 저장
                    }
                    else
                    {
                        Debug.LogError("타일 " + tile.name + "에 대한 guid가 없습니다.");
                    }
                }
            }
            
            tilemapDatas.Add(currentMapData); // 데이터 리스트에 저장
        }
        // 모든 맵 정보를 통째로 Save
        FileHandler.SaveListToJSON<TilemapData>(tilemapDatas, fileName);
    }

    public void OnLoad()
    {
        List<TilemapData> tilemapDatas = FileHandler.ReadListFromJSON<TilemapData>(fileName);

        foreach (TilemapData tilemapData in tilemapDatas)
        {
            string mapName = tilemapData.mapName;
            if (!tilemaps.ContainsKey(mapName))
            {
                Debug.LogError("JSON 파일에 해당하는 타일맵 정보가 없습니다.");
                continue;
            }

            Tilemap currentTilemap = tilemaps[mapName];
            currentTilemap.ClearAllTiles();

            if (tilemapData.tiles is null || tilemapData.tiles.Count <= 0)
            {
                continue;
            }
            foreach (TileInfo currentTile in tilemapData.tiles)
            {
                TileBase tileBase = currentTile.tileBase;
                if (tileBase is null)
                {
                    // tileBase가 저장되어 있지 않다면 AssetDatabase에서 guid를 가지고 로드
                    string guidPath = AssetDatabase.GUIDToAssetPath(currentTile.guidFromAssetDB);
                    tileBase = AssetDatabase.LoadAssetAtPath<TileBase>(guidPath);

                    // 그래도 없으면 없음
                    if (tileBase is null)
                    {
                        Debug.LogError("타일을 AssetDatabase에서 찾을 수 없습니다.");
                        continue;
                    }
                }
                currentTilemap.SetTile(currentTile.position, currentTile.tileBase);
            }
        }
    }
}

[Serializable]
public class TilemapData // 하나의 타일맵은 하나의 tilemapData를 갖는다.
{
    public string mapName; // 이 이름을 가지고 어떤 타일맵에 대한 정보인지를 tilemaps에서 찾을 수 있는 키값의 역할을 함
    public List<TileInfo> tiles = new List<TileInfo>();
}

[Serializable]
public class TileInfo
{
    public TileBase tileBase;
    public Vector3Int position;
    public string guidFromAssetDB;

    public TileInfo(TileBase tileBase, Vector3Int position, string guid) 
    {
        this.tileBase = tileBase;
        this.position = position;
        guidFromAssetDB = guid;
    }
}