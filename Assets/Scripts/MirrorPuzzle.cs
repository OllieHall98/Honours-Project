using System;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using Player.Camera_Controller;
using UI;
using UnityEngine;
using VHS;

public class MirrorPuzzle : MonoBehaviour
{
    public static MirrorPuzzle Instance;

    private GameObject _player;
    
    public Transform targetTransform;
    public GameObject playerCamera;

    public Affective.ProcessWebcamInput webcamScript;
    public Material mirrorMat;
    
    private CameraController _cameraController;
    private FirstPersonController _firstPersonController;
    private InputHandler _inputHandler;

    [SerializeField] private AffectiveManager affectiveManager;
    
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

    private void SetRenderTexture(Color startColor)
    {
        mirrorMat.mainTexture = webcamScript._webcamTex;
        mirrorMat.color = startColor;
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
    private static readonly int MainTex = Shader.PropertyToID("_BaseColorMap");

    private IEnumerator MoveToMirror()
    {
        _coroutineExecuting = true;

        var targetPosition = targetTransform.position;
        var startPosition = _player.transform.position;
        var startForward = playerCamera.transform.forward;
        
        var modifiedTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        var startColor = new Color(2, 2, 2, 1);
        var endColor = new Color(1, 1, 1, 1);
        
        SetRenderTexture(startColor);
        
        for (_ft = 0f; _ft <= 1; _ft += Time.deltaTime / lerpDuration)
        {
            _player.transform.position = Vector3.Lerp(startPosition, modifiedTargetPosition, _ft);
            playerCamera.transform.forward = Vector3.Lerp(startForward, -targetTransform.forward, _ft);
            mirrorMat.color = Color.Lerp(startColor, endColor, _ft);
            
            yield return null;
        }

        StartCoroutine(MirrorPromptSequence());
        
        _coroutineExecuting = false;
    }

    private IEnumerator MirrorPromptSequence()
    {
        NotificationText.Instance.DisplayMessage("Show us a smile!", 3f);

        while (!CheckPlayerEmotion("joy"))
            yield return null;

        NotificationText.Instance.DisplayMessage("You've just seen something shocking!", 3f);
        
        while (!CheckPlayerEmotion("surprise"))
            yield return null;
        
        NotificationText.Instance.DisplayMessage("Congratulations! You've contracted depression! Give us a sad face", 3f);
        
        while (!CheckPlayerEmotion("sadness"))
            yield return null;
        
        // EXIT PUZZLE
        ExitMirror();
    }

    private bool CheckPlayerEmotion(string desiredEmotion)
    {
        return affectiveManager.GetCurrentEmotion() == desiredEmotion;
    }

    private void ExitMirror()
    {
        _cameraController.enabled = true;
        _firstPersonController.enabled = true;
        _inputHandler.enabled = true;
        
        mirrorMat.color = new Color(0, 0, 0, 0);
    }
}
