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

    public Transform playerStartTransform;

    public GameObject cameraPivot;
    
    public Camera playerCamera;

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


    public void SetMovementActive(bool movement, bool cam)
    {
        cameraController.enabled = cam;
        firstPersonController.active = movement;
    }

    public void SetInput(bool value) => inputHandler.enabled = value;

    private void GetPlayerComponents()
    {

        playerCamera = GetComponentInChildren<Camera>();
        
        cameraController = GetComponentInChildren<CameraController>();
        firstPersonController = GetComponent<FirstPersonController>();
        inputHandler = GetComponent<InputHandler>();
    }
    
}
