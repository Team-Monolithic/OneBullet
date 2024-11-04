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
                inst.GetComponent<ActionItem>().MakePropertyUI(action);
                inst.GetComponent<ActionItem>().TargetAction = action;
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
        targetEvent.AddAction(newAction);

        foreach (ActionProperty property in actionSO.properties)
        {
            newAction.properties[property.name] = property;
        }
        
        GameObject inst = Instantiate(actionPrefab);
        inst.transform.SetParent(actionListParent, false);
        inst.GetComponentInChildren<TextMeshProUGUI>().text = actionSO.ActionDisplayName;
        inst.GetComponent<ActionItem>().TargetAction = newAction;
        inst.GetComponent<ActionItem>().AddProperties(actionSO);
        
        actionAddButton.gameObject.SetActive(true);
        actionCategory.SetActive(false);
        actionCategory.GetComponentInChildren<CategoryMenuHandler>().ToggleExpandMode();
    }
}
