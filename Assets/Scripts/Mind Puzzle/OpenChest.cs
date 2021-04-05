using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenChest : MonoBehaviour
{
    public static OpenChest Instance;

    [SerializeField] private MoodReceptorManager _receptorManager;

    [SerializeField] private Animator mindPuzzleAnimator;
    private static readonly int Chest = Animator.StringToHash("OpenChest");

    [SerializeField] private Transform targetTransform;

    public AK.Wwise.Event OpenChestAudio;
    
    public GameObject conveyanceCube;
    
    private GameObject _player, _cameraPivot;
    private Camera _playerCamera;
    
    private float _ft;
    public float lerpDuration = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _player = PlayerStateScript.Instance.gameObject;
        _playerCamera = PlayerStateScript.Instance.playerCamera;
        _cameraPivot = PlayerStateScript.Instance.cameraPivot;
    }

    public void Open()
    {
        if(!_coroutineExecuting) StartCoroutine(MovePlayerToPosition());

        OpenChestAudio.Post(gameObject);
        
        conveyanceCube.SetActive(true);
        
        //mindPuzzleAnimator.SetTrigger(Chest);
        StartCoroutine(TimeAnimation());
        
        
    }
    
    IEnumerator TimeAnimation()
    {
        mindPuzzleAnimator.SetTrigger(Chest);

        yield return new WaitForSecondsRealtime(1.0f);
        
        _receptorManager.StartReceptorAudio();
        
        yield return new WaitForSecondsRealtime(6.0f);
        
        EndCutscene();
    }
    
    public void StartCutscene()
    {
        UIVisibilityScript.Instance.HideUI();
    }

    public void EnableCube()
    {
        conveyanceCube.SetActive(true);
    }
    
    public void EndCutscene()
    {
        _receptorManager.AddReceptorsToDictionary();
        
        ConveyanceCubeScript.Instance.Pickup();
        
        UIVisibilityScript.Instance.ShowUI();
        
        
    }

    private bool _coroutineExecuting;

    private IEnumerator MovePlayerToPosition()
    {
        _coroutineExecuting = true;

        //PlayerStateScript.Instance.SetInput(false);
        PlayerStateScript.Instance.SetMovementActive(false, true);

        
        float timer = 0;
        
        var targetPosition = targetTransform.position;
        var startPosition = _player.transform.position;
        var startForward = _cameraPivot.transform.localEulerAngles;

        var modifiedTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        Vector3 targetForward = new Vector3(0, 0, 0);
        
        while(timer < lerpDuration)
        {
            _player.transform.position = Vector3.Lerp(startPosition, modifiedTargetPosition, timer / lerpDuration);
            _cameraPivot.transform.localEulerAngles = Vector3.Lerp(startForward, targetForward, timer / lerpDuration);
            
            //_playerCamera.transform.forward = Vector3.MoveTowards(startForward, -targetTransform.forward, timer / lerpDuration);
            timer += Time.deltaTime;
            
            yield return null;
        }

        _player.transform.position = modifiedTargetPosition;
        _cameraPivot.transform.localEulerAngles = targetForward;
        //_playerCamera.transform.forward = -targetTransform.forward;

        //PlayerStateScript.Instance.SetInput(true);
        
        
        //PlayerStateScript.Instance.SetMovementActive(true, true);
        
        _coroutineExecuting = false;
    }
    
    
}
