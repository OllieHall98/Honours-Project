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

    public GameObject blackBars;
    
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
        
        
        
        //mindPuzzleAnimator.SetTrigger(Chest);
        StartCoroutine(TimeAnimation());
    }
    
    IEnumerator TimeAnimation()
    {
        blackBars.SetActive(true);
        
        UIVisibilityScript.Instance.HideUI(0.25f);
        
        mindPuzzleAnimator.SetTrigger(Chest);
        
        ReticleManager.Instance.HideReticle();

        yield return new WaitForSecondsRealtime(1.0f);
        
        _receptorManager.StartReceptorAudio();
        
        yield return new WaitForSecondsRealtime(3.0f);
        
        conveyanceCube.SetActive(true);
        
        yield return new WaitForSecondsRealtime(2.75f);
        
        EndCutscene();
    }

    private void EndCutscene()
    {
        blackBars.SetActive(false);
        
        _receptorManager.AddReceptorsToDictionary();
        
        ConveyanceCubeScript.Instance.Pickup();
        
        UIVisibilityScript.Instance.ShowUI(0.5f);

        //PlayerStateScript.Instance.cameraController.SetDesiredPitch(0);
    }

    private bool _coroutineExecuting;

    private IEnumerator MovePlayerToPosition()
    {
        _coroutineExecuting = true;
        
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
            timer += Time.deltaTime;
            
            yield return null;
        }

        _player.transform.position = modifiedTargetPosition;
        _cameraPivot.transform.localEulerAngles = targetForward;

        _coroutineExecuting = false;
    }
    
    
}
