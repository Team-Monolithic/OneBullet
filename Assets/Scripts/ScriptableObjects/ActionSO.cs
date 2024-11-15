using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// public enum ActionType
// {
//     FireTrigger,
//     MoveTo,
//     Attack
// }

// [CreateAssetMenu (fileName = "Action", menuName = "Scripting/Create Action")]
public abstract class ActionSO : ScriptableObject
{
    [SerializeField] private ActionCategorySO actionCategory;
    // [SerializeField] private ActionType actionType;
    [SerializeField] private string actionDisplayName;
    public ActionProperty[] properties;
    
    public ActionCategorySO ActionCategory => actionCategory;

    // public ActionType ActionType => actionType;
    public string ActionDisplayName => actionDisplayName;

    public abstract void Execute(in Dictionary<string, ActionProperty> properties, ref BuildingTile ownerObject);
}

[System.Serializable]
public class ActionProperty
{
    public string name;
    public ActionPropertyType type;
    public object value;

    public enum ActionPropertyType
    {
        Float,
        Int,
    }
}