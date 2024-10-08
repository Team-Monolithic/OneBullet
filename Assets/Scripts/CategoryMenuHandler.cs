using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryMenuHandler : MonoBehaviour
{
    [SerializeField] private GameObject shrink;
    [SerializeField] private GameObject expand;
    [SerializeField] private GameObject items;
    private bool isExpanded = false;
    
    public void ToggleExpandMode()
    {
        isExpanded = !isExpanded;
        if (isExpanded == true)
        {
            shrink.SetActive(true);
            expand.SetActive(false);
            items.SetActive(true);
        }
        else
        {
            shrink.SetActive(false);
            expand.SetActive(true);
            items.SetActive(false);
        }
        EventHandler.GetInstance().RebuildLayout();
    }
}
