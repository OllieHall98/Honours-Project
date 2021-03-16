using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRaycast : MonoBehaviour
{
    private Camera _cam;
    public float raycastDistance;

    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        FireRay();
    }
    
    void FireRay()
    {
        if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out var hit, raycastDistance)) return;
        
        switch (hit.transform.name)
        {
            case "Mirror":
                Mirror();
                break;
        }
    }

    void Mirror()
    {
        if (Input.GetButtonDown($"Interact"))
        {


            Debug.Log("Yeet");
        }
    }
    
}
