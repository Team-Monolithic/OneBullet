using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ScriptingManager : Singleton<ScriptingManager>
{
    [SerializeField] private GameObject tileImage;
    [SerializeField] private GameObject tileName;

    public void SetTileInfo(BuildingObjectBase buildingObjectBase)
    {
        Tile tile = (Tile)buildingObjectBase.TileBase;
        tileImage.GetComponent<Image>().sprite = tile.sprite;
        tileName.GetComponent<TextMeshProUGUI>().text = buildingObjectBase.DisplayName;
    }
}
