using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventItem : MonoBehaviour
{
    [SerializeField] private GameObject actionAddButton;
    [SerializeField] private GameObject actionCategory;
    [SerializeField] private GameObject actionPrefab;
    [SerializeField] private Transform actionListParent;
    
    public void ActionAddButtonClicked()
    {
        actionCategory.SetActive(true);
        actionAddButton.gameObject.SetActive(false);
        EventHandler.GetInstance().RebuildLayout();
    }

    public void ActionCategoryButtonClicked()
    {
        GameObject inst = Instantiate(actionPrefab);
        inst.transform.SetParent(actionListParent, false);
        
        actionAddButton.gameObject.SetActive(true);
        actionCategory.SetActive(false);
        actionCategory.GetComponentInChildren<CategoryMenuHandler>().ToggleExpandMode();
        EventHandler.GetInstance().RebuildLayout();
    }
    
}
