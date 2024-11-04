using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PropertyItem : MonoBehaviour
{
    [SerializeField] public TMP_InputField XInputField;
    public ActionProperty targetProperty;
    public Action ownerAction;

    private void Awake()
    {
        XInputField.onEndEdit.AddListener(OnEndEdit);
    }

    // 인풋 박스에 입력된 값을 프로퍼티 리스트에 저장
    private void OnEndEdit(string value)
    {
        if (targetProperty.type == ActionProperty.PropertyType.Float)
        {
            if (float.TryParse(value, out float result))
            {
                targetProperty.value = result;
                ownerAction.properties[targetProperty.name] = targetProperty;
            }
        }
    }
}
