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

    // TODO : ActionItem script 만들어서 targetAction 붙이고 실제 액션과 연결
    
    public void AddActionCategory()
    {
        GameObject inst = Instantiate(ScriptingManager.GetInstance().actionCategoryTemp);
        inst.transform.SetParent(actionCategory.transform);
        inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        foreach (Transform child in inst.transform)
        {
            GameObject items = child.transform.Find("Items").gameObject;
            foreach (Button btn in items.GetComponentsInChildren<Button>())
            {
                btn.onClick.AddListener(() =>
                {
                    ActionCategoryButtonClicked(btn.gameObject.GetComponent<ActionHandler>()._actionSO);                });
            }
        }
    }

    public void ActionAddButtonClicked()
    {
        actionAddButton.gameObject.SetActive(false);
        actionCategory.gameObject.SetActive(true);
    }

    public void ActionCategoryButtonClicked(ActionSO actionSO)
    {
        Action newAction = new Action();
        newAction.actionType = actionSO.SOActionType;
        
        // 해당 event에 action 추가
        targetEvent.AddAction(newAction);
        
        GameObject inst = Instantiate(actionPrefab);
        inst.transform.SetParent(actionListParent, false);
        inst.GetComponentInChildren<TextMeshProUGUI>().text = actionSO.ActionDisplayName;
        actionAddButton.gameObject.SetActive(true);
        actionCategory.SetActive(false);
        actionCategory.GetComponentInChildren<CategoryMenuHandler>().ToggleExpandMode();
    }
}
