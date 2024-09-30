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
    private Button btn;
    private bool isExpanded = false;
    

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(TitleClicked);
        ToggleExpandMode();
    }

    private void TitleClicked()
    {
        isExpanded = !isExpanded;
        ToggleExpandMode();
    }

    private void ToggleExpandMode()
    {
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
    }
}
