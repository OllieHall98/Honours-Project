using System;
using System.Collections;
using System.Collections.Generic;
using Player.Camera_Controller;
using UnityEngine;
using VHS;

public class MirrorPuzzle : MonoBehaviour
{
    public static MirrorPuzzle Instance;

    private GameObject _player;
    
    public Transform targetTransform;
    public GameObject playerCamera;
    
    private CameraController _cameraController;
    private FirstPersonController _firstPersonController;
    private InputHandler _inputHandler;
    
    private float _ft;
    
    public float lerpDuration = 0.5f;
    
    

    private void Awake()
    {
        Instance = this;
        _player = GameObject.FindWithTag("Player");

        GetPlayerComponents();
    }

    private void GetPlayerComponents()
    {
        _cameraController = _player.GetComponentInChildren<CameraController>();
        _firstPersonController = _player.GetComponent<FirstPersonController>();
        _inputHandler = _player.GetComponent<InputHandler>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initiate()
    {
        // Move player in front of mirror

        _cameraController.enabled = false;
        _firstPersonController.enabled = false;
        _inputHandler.enabled = false;
        
        if (!_coroutineExecuting) StartCoroutine(MoveToMirror());
    }


    private bool _coroutineExecuting;

    private IEnumerator MoveToMirror()
    {
        _coroutineExecuting = true;

        var targetPosition = targetTransform.position;
        var startPosition = _player.transform.position;
        var startForward = _player.transform.forward;
        
        var modifiedTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);
        
        for (_ft = 0f; _ft <= 1; _ft += Time.deltaTime / lerpDuration)
        {
            _player.transform.position = Vector3.Lerp(startPosition, modifiedTargetPosition, _ft);
            _player.transform.forward = Vector3.Lerp(startForward, -targetTransform.forward, _ft);
            yield return null;
        }

        _coroutineExecuting = false;
    }
}
