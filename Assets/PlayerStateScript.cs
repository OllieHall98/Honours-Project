using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake() => Instance = this;

    public void SetPlayerState(PlayerState state) => _playerState = state;
    public PlayerState GetPlayerState() => _playerState;
    
    public void SetRaycastState(RaycastState state) => _raycastState = state;
    public RaycastState GetRaycastState() => _raycastState;
}
