using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum ActionType
{
    FireTrigger,
    MoveTo
}

[CreateAssetMenu (fileName = "Action", menuName = "Scripting/Create Action")]
public class ActionSO : ScriptableObject
{
    [SerializeField] private ActionCategorySO actionCategory;
    [SerializeField] private ActionType actionType;
    [SerializeField] private string actionDisplayName;
    public ActionProperty[] properties;
    
    public ActionCategorySO ActionCategory => actionCategory;

    public ActionType SOActionType => actionType;
    public string ActionDisplayName => actionDisplayName;
}

[System.Serializable]
public class ActionProperty
{
    public string name;
    public PropertyType type;
    public object value;

    public enum PropertyType
    {
        Float,
        Int,
        Vector2
    }
}