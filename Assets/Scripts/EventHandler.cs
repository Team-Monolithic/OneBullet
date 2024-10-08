using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventHandler : Singleton<EventHandler>
{
    [SerializeField] private RectTransform layoutGroup;
    [SerializeField] private Button eventAddButton;
    
    [SerializeField] private Transform eventListParent;
    [SerializeField] private GameObject eventCategory;
    [SerializeField] private GameObject eventPrefab;

    private void Update()
    {
        RebuildLayout(); // 수정 필요
    }

    public void EventAddButtonClicked()
    {
        eventCategory.SetActive(true);
        eventAddButton.gameObject.SetActive(false);
        RebuildLayout();
    }

    public void EventCategoryButtonClicked()
    {
        GameObject inst = Instantiate(eventPrefab);
        inst.transform.SetParent(eventListParent, false);
        
        eventListParent.gameObject.SetActive(true);
        eventAddButton.gameObject.SetActive(true);
        
        RebuildLayout();
        eventCategory.SetActive(false);
        eventCategory.GetComponent<CategoryMenuHandler>().ToggleExpandMode();
        RebuildLayout();
    }

    public void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup);
    }
}
