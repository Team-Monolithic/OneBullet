using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class EventPropertyItem : MonoBehaviour
{
    private EventPublisher eventPublisher;
    [SerializeField] public TMP_InputField InputField;
    public EventProperty targetProperty;
    public Event ownerEvent;
    private int prevTriggerIdx = 0;
    
    private void Awake()
    {
        InputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void Start()
    {
        eventPublisher = EventPublisher.GetInstance();
    }

    // 인풋 박스에 입력된 값을 프로퍼티 리스트에 저장
    private void OnEndEdit(string value)
    {
        if (targetProperty.type == EventProperty.EventPropertyType.Int)
        {
            if (int.TryParse(value, out int result))
            {
                targetProperty.value = result; 
                ownerEvent.properties[targetProperty.name] = targetProperty;
                if (ownerEvent.eventSO.EventType == EventType.TriggerFired)
                {
                    foreach (Action action in ownerEvent.actions) // 이전 번호의 델리게이트에서 unregister
                    {
                        eventPublisher.UnregisterTrigger(prevTriggerIdx, action.executeAction);
                    }
                    
                    foreach (Action action in ownerEvent.actions) // 현재 번호의 델리게이트에 register
                    {
                        eventPublisher.RegisterTrigger(result, action.executeAction);
                    }
                }
                
                prevTriggerIdx = result;
            }
        }
    }
}
