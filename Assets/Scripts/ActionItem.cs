using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionItem : MonoBehaviour
{
    [SerializeField] private Transform propertyListParent; // 프로퍼티의 부모
    [SerializeField] private GameObject propertyPrefab;
    
    private void Awake()
    {
        
    }

    public void MakePropertyUI(Action action)
    {
        if (action.actionSO.properties.Length > 0)
        {
            foreach (ActionProperty property in action.actionSO.properties)
            {
                GameObject inst = Instantiate(propertyPrefab);
                inst.transform.SetParent(propertyListParent.transform);
                inst.GetComponentInChildren<TextMeshProUGUI>().text = property.name;
                inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        }
    }
    
    public void AddProperties(ActionSO actionSO)
    {
        foreach (ActionProperty property in actionSO.properties)
        {
            GameObject inst = Instantiate(propertyPrefab);
            inst.transform.SetParent(propertyListParent.transform);
            inst.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            inst.GetComponentInChildren<TextMeshProUGUI>().text = property.name;
        }
    }
}
