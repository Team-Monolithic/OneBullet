using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionItem : MonoBehaviour
{
    [SerializeField] private Transform propertyListParent; // 프로퍼티의 부모
    [SerializeField] private GameObject propertyPrefab;

    public Action targetAction;
    
    public void MakePropertyUI(Action action) // 저장된 프로퍼티를 ui 출력
    {
        if (targetAction.actionSO.properties.Length > 0)
        {
            foreach (ActionProperty property in action.actionSO.properties)
            {
                GameObject inst = Instantiate(propertyPrefab);
                inst.transform.SetParent(propertyListParent.transform);
                inst.GetComponentInChildren<TextMeshProUGUI>().text = property.name;
                inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                inst.GetComponent<ActionPropertyItem>().ownerAction = action;
                inst.GetComponent<ActionPropertyItem>().targetProperty = property;
                
                if (targetAction.properties.ContainsKey(property.name)) // 프로퍼티의 값이 있다면 불러온다
                {
                    TMP_InputField inputField = inst.GetComponentInChildren<TMP_InputField>();
                    inputField.text = targetAction.properties[property.name].value.ToString();
                    inputField.ForceLabelUpdate();
                }
            }
        }
    }
    
    public void AddProperties(ActionSO actionSO) // 프로퍼티 처음에 생성
    {
        foreach (ActionProperty property in actionSO.properties)
        {
            GameObject inst = Instantiate(propertyPrefab);
            inst.transform.SetParent(propertyListParent.transform);
            inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            inst.GetComponentInChildren<TextMeshProUGUI>().text = property.name;
            inst.GetComponent<ActionPropertyItem>().ownerAction = targetAction;
            inst.GetComponent<ActionPropertyItem>().targetProperty = property;
        }
    }
}
