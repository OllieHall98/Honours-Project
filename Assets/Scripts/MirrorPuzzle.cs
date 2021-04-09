using System;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using Player.Camera_Controller;
using UI;
using UnityEngine;
using UnityEngine.UI;
using VHS;

public class MirrorPuzzle : MonoBehaviour
{
    public static MirrorPuzzle Instance;

    private GameObject _player;
    
    public Image fader;
    
    public Transform targetTransform;
    public GameObject playerCamera;

    [SerializeField] private Animator cutsceneAnimator;

    public Affective.ProcessWebcamInput webcamScript;
    //public Material mirrorMat;
    
    [SerializeField] private AK.Wwise.Event RelicGet;
    
    [SerializeField] private GameObject soulBeacon;


    [SerializeField] private AffectiveManager affectiveManager;
    
    private float _ft;
    
    public float lerpDuration = 0.5f;
    
    private void Awake()
    {
        Instance = this;
        _player = GameObject.FindWithTag("Player");
    }
    
    

    public void Initiate()
    {
        // Move player in front of mirror
        PlayerStateScript.Instance.SetMovementActive(false, true);

        if (!_coroutineExecuting) StartCoroutine(MoveToMirror());
    }


    private bool _coroutineExecuting;
    private static readonly int MainTex = Shader.PropertyToID("_BaseColorMap");
    private static readonly int FinishedPuzzle = Animator.StringToHash("FinishedPuzzle");

    private IEnumerator MoveToMirror()
    {
        _coroutineExecuting = true;

        UIVisibilityScript.Instance.HideUI(1.0f);

        var targetPosition = targetTransform.position;
        var startPosition = _player.transform.position;
        var startForward = playerCamera.transform.forward;
        
        var modifiedTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);

        var startColor = new Color(2, 2, 2, 1);
        var endColor = new Color(1, 1, 1, 1);

        for (_ft = 0f; _ft <= 1; _ft += Time.deltaTime / lerpDuration)
        {
            _player.transform.position = Vector3.Lerp(startPosition, modifiedTargetPosition, _ft);
            playerCamera.transform.forward = Vector3.Lerp(startForward, -targetTransform.forward, _ft);

            yield return null;
        }

        StartCoroutine(MirrorPromptSequence());
        
        _coroutineExecuting = false;
    }

    private IEnumerator MirrorPromptSequence()
    {
        // NotificationText.Instance.DisplayMessage(TransitionType.Float, "<color=black>Show us a smile!</color>", 3f);
        //
        // while (!CheckPlayerEmotion("joy"))
        //     yield return null;
        //
        // NotificationText.Instance.DisplayMessage(TransitionType.Float,"<color=black>You've just seen something shocking!</color>", 3f);
        //
        // while (!CheckPlayerEmotion("surprise"))
        //     yield return null;
        //
        // NotificationText.Instance.DisplayMessage(TransitionType.Float,"<color=black>Congratulations! You've contracted depression! Give us a sad face</color>", 3f);
        //
        // while (!CheckPlayerEmotion("sadness"))
        //     yield return null;

        yield return new WaitForSecondsRealtime(2.0f);
        
        // EXIT PUZZLE
        StartCoroutine(PuzzleCompleteCutscene());
    }

    private IEnumerator PuzzleCompleteCutscene()
    {

        BlackBarTransitioner.Instance.Show(1.5f);

        yield return new WaitForSecondsRealtime(1.5f);
        
        PlayerStateScript.Instance.SetMovementActive(false, false);
        
        cutsceneAnimator.SetTrigger(FinishedPuzzle);
        RelicGet.Post(gameObject);
        
        yield return new WaitForSecondsRealtime(5.0f);
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float, "<color=yellow>Soul Relic</color> obtained.", 5.0f);
        
        yield return new WaitForSecondsRealtime(5.0f);
        
        var tweenOpacity = LeanTween.value(fader.gameObject, 0, 1, 2f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        
        yield return new WaitForSecondsRealtime(3.1f);
        
        soulBeacon.SetActive(false);
        RelicHolderScript.Instance.EnableRelic(RelicHolderScript.Instance.soulRelic);
        RelicUIManager.Instance.EnableRelicUI(RelicUIManager.Instance.soulRelic);
        _player.transform.position = PlayerStateScript.Instance.playerStartTransform.position;
        
        tweenOpacity = LeanTween.value(fader.gameObject, 1, 0, 2f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        
        PlayerStateScript.Instance.SetRaycastState(RaycastState.Enabled);
        
        PlayerStateScript.Instance.SetMovementActive(true, true);
        yield return new WaitForSecondsRealtime(2f);
        BlackBarTransitioner.Instance.Hide(2f);
        UIVisibilityScript.Instance.ShowUI(2.0f);
        
    }

    private bool CheckPlayerEmotion(string desiredEmotion)
    {
        return affectiveManager.GetCurrentEmotion() == desiredEmotion;
    }

    private void ExitMirror()
    {
        PlayerStateScript.Instance.SetMovementActive(true, true);
    }
}
