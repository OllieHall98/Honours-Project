using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRaycast : MonoBehaviour
{
    private Camera _cam;
    public float raycastDistance;

    private PlayerStateScript _playerStateScript;

    private void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        _playerStateScript = PlayerStateScript.Instance;
    }

    private void Update()
    {
        FireRay();
    }

    private void FireRay()
    {
        if (_playerStateScript.GetRaycastState() == RaycastState.Disabled) return;
        
        if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out var hit, raycastDistance)) return;
        
        HandleRaycastHit(hit);
    }

    private void HandleRaycastHit(RaycastHit hit)
    {
        switch (hit.transform.name)
        {
            case "Mirror":
                if (!Input.GetButtonDown($"Interact")) return;
                
                _playerStateScript.SetRaycastState(RaycastState.Disabled);
                MirrorPuzzle.Instance.Initiate();
                break;
            case "ConveyanceCube":
                if (!Input.GetButtonDown($"Interact")) return;
                
                _playerStateScript.SetRaycastState(RaycastState.Disabled);
                ConveyanceCubeScript.Instance.Pickup();
                break;
        }
    }

}
