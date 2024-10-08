using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ClearTileGrid : MonoBehaviour
{
    private Button button;
    private Tilemap[] maps;
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ClearMap);
    }

    private void Start()
    {
        maps = FindObjectsOfType<Tilemap>();
    }

    private void ClearMap()
    {
        foreach (Tilemap map in maps)
        {
            map.ClearAllTiles();
        }
    }
}
