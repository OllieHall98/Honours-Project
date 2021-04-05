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

    [SerializeField] private AffectiveManager affectiveManager;
    
    private float _ft;
    
    public float lerpDuration = 0.5f;
    
    private void Awake()
    {
        Instance = this;
        _player = GameObject.FindWithTag("Player");
    }
    

    private void SetRenderTexture(Color startColor)
    {
        mirrorMat.mainTexture = webcamScript._webcamTex;
        mirrorMat.color = startColor;
    }
    

    public void Initiate()
    {
        // Move player in front of mirror
        PlayerStateScript.Instance.SetMovementActive(false, false);

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
        NotificationText.Instance.DisplayMessage(TransitionType.Float, "Show us a smile!", 3f);

        while (!CheckPlayerEmotion("joy"))
            yield return null;

        NotificationText.Instance.DisplayMessage(TransitionType.Float,"You've just seen something shocking!", 3f);
        
        while (!CheckPlayerEmotion("surprise"))
            yield return null;
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float,"Congratulations! You've contracted depression! Give us a sad face", 3f);
        
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
        PlayerStateScript.Instance.SetMovementActive(true, true);
        
        mirrorMat.color = new Color(0, 0, 0, 0);
    }
}
