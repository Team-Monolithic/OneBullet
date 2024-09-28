using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ClearTileGrid : MonoBehaviour
{
    private Button button;
    [SerializeField] private List<Tilemap> tilemaps;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ClearMap);
    }

    private void ClearMap()
    {
        foreach (Tilemap map in tilemaps)
        {
            map.ClearAllTiles();
        }
    }
}
