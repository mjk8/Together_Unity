using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    //minView to limit amount of Y-axis view. Probably should be moved elsewhere later
    public float minViewDistance = 15f;
    
    //this is to be fetched from player settings
    static float mouseSensitivity;

    static int sensitivityAdjuster = 3;
    
    private Vector3 _playerPos;
    private float rotationX = 0f;

    private void Start()
    {
        mouseSensitivity = Managers.Data.Player.MouseSensitivity *sensitivityAdjuster;
    }

    private void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -70f, minViewDistance);
        
        transform.GetChild(0).transform.localRotation = Quaternion.Euler(rotationX,0f,0f);
        transform.Rotate(3f * mouseX * Vector3.up);
        if (!PlayerMovement._playerIsMoving)
        {
            transform.GetChild(1).transform.Rotate(3f * -mouseX * Vector3.up);
        }

    }

    public static void MouseSensitivityChanged(float change)
    {
        mouseSensitivity = change*sensitivityAdjuster;
    }
    /*
     _playerPos = transform.parent.GetChild(1).transform.position;
     .
     .
     .
     Using RotateAround
     
     transform.RotateAround(_playerPos,Vector3.up, mouseX);
        Vector3 newPos = new Vector3(_playerPos.x-transform.position.x,transform.position.y,_playerPos.z-transform.position.z);
        Vector3 q = Quaternion.LookRotation(newPos).eulerAngles;
        transform.rotation = Quaternion.Euler(rotationX, q.y, 0);
     */
}
