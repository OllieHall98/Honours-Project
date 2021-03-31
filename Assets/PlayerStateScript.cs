using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VHS;
using Player.Camera_Controller;

public enum PlayerState
{
    Default
}

public enum RaycastState
{
    Enabled,
    Disabled
}

public class PlayerStateScript : MonoBehaviour
{
    public static PlayerStateScript Instance;

    private PlayerState _playerState;
    private RaycastState _raycastState;
    
    [HideInInspector] public CameraController cameraController;
    [HideInInspector] public FirstPersonController firstPersonController;
    [HideInInspector] public InputHandler inputHandler;

    private void Awake()
    {
        Instance = this;
        GetPlayerComponents();
    }

    public void SetPlayerState(PlayerState state) => _playerState = state;
    public PlayerState GetPlayerState() => _playerState;
    
    public void SetRaycastState(RaycastState state) => _raycastState = state;
    public RaycastState GetRaycastState() => _raycastState;


    public void SetMovementActive(bool movement, bool camera)
    {
        cameraController.enabled = camera;
        firstPersonController.active = movement;
    }
    
    private void GetPlayerComponents()
    {
        cameraController = GetComponentInChildren<CameraController>();
        firstPersonController = GetComponent<FirstPersonController>();
        inputHandler = GetComponent<InputHandler>();
    }
    
}
