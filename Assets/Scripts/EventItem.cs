using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventItem : MonoBehaviour
{
    [SerializeField] private GameObject actionAddButton;
    [SerializeField] private GameObject actionCategory;
    [SerializeField] private GameObject actionPrefab;
    
    [SerializeField] private Transform actionListParent;
    [SerializeField] public TextMeshProUGUI eventTitleText;

    [SerializeField] private Transform propertyListParent;
    [SerializeField] private GameObject propertyPrefab;
    
    private Event targetEvent;
    public Event TargetEvent
    {
        get => targetEvent;
        set => targetEvent = value;
    }
    
    public void AddActionCategory()
    {
        GameObject inst = Instantiate(ScriptingManager.GetInstance().actionCategoryTemp);
        inst.SetActive(true);
        inst.transform.SetParent(actionCategory.transform);
        inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        foreach (Transform child in inst.transform)
        {
            GameObject items = child.transform.Find("Items").gameObject;
            foreach (Button btn in items.GetComponentsInChildren<Button>())
            {
                btn.onClick.AddListener(() =>
                {
                    ActionCategoryButtonClicked(btn.gameObject.GetComponent<ActionHandler>()._actionSO);                
                });
            }
        }
    }

    public void MakePropertyUI(Event tileEvent)
    {
        if (tileEvent.eventSO.properties.Length > 0)
        {
            foreach (EventProperty property in tileEvent.eventSO.properties)
            {
                GameObject inst = Instantiate(propertyPrefab);
                inst.transform.SetParent(propertyListParent);
                inst.GetComponentInChildren<TextMeshProUGUI>().text = property.name;
                inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                inst.GetComponent<EventPropertyItem>().ownerEvent = tileEvent;
                inst.GetComponent<EventPropertyItem>().targetProperty = property;
                
                if (targetEvent.properties.ContainsKey(property.name))
                {
                    TMP_InputField inputField = inst.GetComponentInChildren<TMP_InputField>();
                    if (inputField != null)
                    {
                        inputField.text = targetEvent.properties[property.name].value.ToString();
                        inputField.ForceLabelUpdate();
                    }
                }
            }
        }
    }

    public void MakeActionUI(Event tileEvent) // 특정 이벤트의 액션들을 ui 출력
    {
        if (tileEvent.actions.Count > 0)
        {
            foreach (Action action in tileEvent.actions)
            {
                GameObject inst = Instantiate(actionPrefab);
                inst.transform.SetParent(actionListParent);
                inst.GetComponentInChildren<TextMeshProUGUI>().text = action.actionSO.ActionDisplayName;
                inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                inst.GetComponent<ActionItem>().targetAction = action;
                inst.GetComponent<ActionItem>().MakePropertyUI(action);
            }
        }
    }

    public void ActionAddButtonClicked()
    {
        actionAddButton.gameObject.SetActive(false);
        actionCategory.gameObject.SetActive(true);
    }

    public void ActionCategoryButtonClicked(ActionSO actionSO) // 새로운 액션 생성
    {
        Action newAction = new Action();
        newAction.actionSO = actionSO;
        newAction.ownerEvent = targetEvent;
        targetEvent.AddAction(newAction);
        
        GameObject inst = Instantiate(actionPrefab);
        inst.transform.SetParent(actionListParent, false);
        inst.GetComponentInChildren<TextMeshProUGUI>().text = actionSO.ActionDisplayName;
        inst.GetComponent<ActionItem>().targetAction = newAction;
        inst.GetComponent<ActionItem>().AddProperties(actionSO);
        
        actionAddButton.gameObject.SetActive(true);
        actionCategory.SetActive(false);
        actionCategory.GetComponentInChildren<CategoryMenuHandler>().ToggleExpandMode();
        
        if (targetEvent.eventSO.EventType == EventType.TriggerFired) // targetEvent (부모 이벤트)가 트리거 발생 이벤트이고, 트리거 번호가 null이 아니라면, 해당 액션을 트리거 번호에 바인딩
        {
            if (targetEvent.properties.ContainsKey("TriggerNumber"))
            {
                int triggerNumber = (int)targetEvent.properties["TriggerNumber"].value;
                EventPublisher.GetInstance().RegisterTrigger(triggerNumber, newAction.executeAction);
            }
        }
    }
}
