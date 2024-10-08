using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "UICategory", menuName = "LevelBuilding/Create UI Category")]
public class UICategory : ScriptableObject
{
    [SerializeField] private int siblingIndex = 0;
    [SerializeField] private Color backgroundColor;

    public int SiblingIndex => siblingIndex;
    public Color BackgroundColor => backgroundColor;
}
