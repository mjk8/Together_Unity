using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class DetectorCamera : MonoBehaviour
{
    public Camera targetCamera;
    public Camera mainCamera;
    public Shader replacementShader; // The replacement shader that will render the specific GameObject

    private int originalLayer; // Store the original layer of the GameObject
    public bool _isDetecting = false;

    void OnEnable()
    {
        if (!Managers.Player.IsMyDediPlayerKiller())
        {
            enabled = false;
        }
        mainCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = transform.GetComponent<Camera>();
        }
        if (replacementShader == null)
        {
            Debug.LogError("Replacement shader not assigned! Please assign a shader.");
            return;
        }
        // Synchronize camera properties
        targetCamera.fieldOfView = mainCamera.fieldOfView;
        targetCamera.nearClipPlane = mainCamera.nearClipPlane;
        targetCamera.farClipPlane = mainCamera.farClipPlane;
        targetCamera.orthographic = mainCamera.orthographic;
        targetCamera.SetReplacementShader(replacementShader, "");
    }

    void Update()
    {
        if (_isDetecting)
        {
            transform.rotation = mainCamera.transform.rotation;
            Vector3 relativePosition = mainCamera.transform.position - transform.position;
            transform.position += relativePosition;
        }
        else
        {
            transform.rotation = mainCamera.transform.rotation;
            transform.position = mainCamera.transform.position;
        }
    }
    
    void OnDisable()
    {
        // Reset the camera's shader replacement when the script is disabled
        if (targetCamera != null)
        {
            targetCamera.ResetReplacementShader();
        }
    }
}
