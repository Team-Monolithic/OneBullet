using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainAspectRatio : MonoBehaviour
{
    private Camera cam;
    private const float targetAspect = 16f / 9f;

    private void UpdateCamera()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f) // 세로 여백
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else // 가로 여백 
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }

    void Start()
    {
        cam = Camera.main;
        UpdateCamera();
    }

    void Update()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        if (Mathf.Abs(windowAspect - 1f) > 0.0001)
        {
            UpdateCamera();
        }
    }
}
