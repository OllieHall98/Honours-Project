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
        
        HandleRaycastHit(hit);
    }

    void HandleRaycastHit(RaycastHit hit)
    {
        switch (hit.transform.name)
        {
            case "Mirror":
                if (Input.GetButtonDown($"Interact")) MirrorPuzzle.Instance.Initiate();
                    break;
            case "ConveyanceCube":
                if (Input.GetButtonDown($"Interact")) ConveyanceCubeScript.Instance.Pickup();
                break;
        }
    }

}
