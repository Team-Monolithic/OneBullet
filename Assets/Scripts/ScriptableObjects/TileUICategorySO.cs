using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "TileCategorySO", menuName = "LevelBuilding/Create Tile Category")]
public class TileUICategorySO : ScriptableObject
{
    [SerializeField] private int siblingIndex = 0;
    [SerializeField] private Color backgroundColor;

    public int SiblingIndex => siblingIndex;
    public Color BackgroundColor => backgroundColor;
}
