using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    Started,
    TriggerFired,
    Interacted
}

[CreateAssetMenu (fileName = "Event", menuName = "Scripting/Create Event")]
public class EventSO : ScriptableObject
{
    [SerializeField] private EventCategorySO eventCategory;
    [SerializeField] private EventType eventType;
    [SerializeField] private string eventDisplayName;
    
    public EventCategorySO EventCategory => eventCategory;
    public EventType EventType => eventType;
    public string EventDisplayName => eventDisplayName;
}
