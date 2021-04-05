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

        if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out var hit, raycastDistance))
        {
            ReticleManager.Instance.HideReticle();
            return;
        }
        
        HandleRaycastHit(hit);
    }

    private void HandleRaycastHit(RaycastHit hit)
    {
        ReticleManager.Instance.ShowReticle();
        
        switch (hit.transform.name)
        {
            case "Mirror":
                if (!Input.GetButtonDown($"Interact")) return;
                _playerStateScript.SetRaycastState(RaycastState.Disabled);
                ReticleManager.Instance.HideReticle();
                MirrorPuzzle.Instance.Initiate();
                break;
            case "ConveyanceCube":
                // if (!Input.GetButtonDown($"Interact")) return;
                // _playerStateScript.SetRaycastState(RaycastState.Disabled);
                // ReticleManager.Instance.HideReticle();
                // ConveyanceCubeScript.Instance.Pickup();
                break;
            case "Receptor Chest Cap":
                if (!Input.GetButtonDown($"Interact")) return;
                ReticleManager.Instance.HideReticle();
                OpenChest.Instance.Open();
                break;
            default:
                ReticleManager.Instance.HideReticle();
                break;
        }
    }

}
