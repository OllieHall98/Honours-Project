using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class FireRaycast : MonoBehaviour
{
    private Camera _cam;
    public float raycastDistance;

    private PlayerStateScript _playerStateScript;

    public LayerMask mask;

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

        if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, out var hit, raycastDistance, mask))
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
            case "Receptor Chest Cap":
                if (!Input.GetButtonDown($"Interact")) return;
                ReticleManager.Instance.HideReticle();
                OpenChest.Instance.Open();
                break;
            case "Maze Instructions":
                if (!Input.GetButtonDown($"Interact")) return;
                ReticleManager.Instance.HideReticle();
                MazePickup.Instance.Pickup();
                break;
            case "RelicCollider":
                if (!Input.GetButtonDown($"Interact")) return;
                NotificationText.Instance.DisplayMessage(TransitionType.Float, "<size=40>Find my <b><color=#40E0D0>Mind</color></b>.. <b><color=red>Body</color></b>.. and <b><color=yellow>Soul</color></b>..      follow the lights...</size>", 8f);
                break;
            default:
                ReticleManager.Instance.HideReticle();
                break;
        }
    }

}
