using System;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using Player.Camera_Controller;
using UI;
using UnityEngine;
using UnityEngine.UI;
using VHS;
using Weather;

public class MirrorPuzzle : MonoBehaviour
{
    public static MirrorPuzzle Instance;

    private GameObject _player;
    
    public Image fader;
    
    public Transform targetTransform;
    public GameObject playerCamera;

    [SerializeField] private Animator cutsceneAnimator;

    public Affective.ProcessWebcamInput webcamScript;

    [SerializeField] private AK.Wwise.Event relicGet;
    [SerializeField] private AK.Wwise.Event mirrorPromptComplete;
    [SerializeField] private AK.Wwise.Event mirrorSparkle;
    [SerializeField] private AK.Wwise.RTPC mirrorPromptPitch;
    
    [SerializeField] private GameObject soulBeacon;

    [SerializeField] private Color color0, color1, color2, color3;
    [SerializeField] private GameObject mirrorMat;

    [SerializeField] private AffectiveManager affectiveManager;

    [SerializeField] private string happySentence, surpriseSentence, sadSentence;
    
    [SerializeField] private AK.Wwise.Event ambientMusic;
    
    private float _ft;
    
    public float lerpDuration = 0.5f;
    
    private void Awake()
    {
        Instance = this;
        _player = GameObject.FindWithTag("Player");
        
        mirrorMat.GetComponent<Renderer>().materials[1].SetColor(EmissiveColor, color0);
    }
    
    

    public void Initiate()
    {
        // Move player in front of mirror
        PlayerStateScript.Instance.SetMovementActive(false, true);

        mirrorSparkle.Post(gameObject);
        
        if (!_coroutineExecuting) StartCoroutine(MoveToMirror());
    }


    private bool _coroutineExecuting;
    private static readonly int MainTex = Shader.PropertyToID("_BaseColorMap");
    private static readonly int FinishedPuzzle = Animator.StringToHash("FinishedPuzzle");
    private static readonly int EmissiveColor = Shader.PropertyToID("_EmissiveColor");

    private IEnumerator MoveToMirror()
    {
        _coroutineExecuting = true;
        

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
        WeatherController.Instance.enabled = false;
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float, happySentence, 0);
        
        while (!CheckPlayerEmotion("joy"))
            yield return null;

        NotificationText.Instance.StopDisplayingText(TransitionType.Float);
        yield return new WaitForSecondsRealtime(0.5f);
        
        mirrorPromptPitch.SetValue(gameObject, 0);
        mirrorPromptComplete.Post(gameObject);
        
        var material = mirrorMat.GetComponent<Renderer>().materials[1];
        
        var tweenEmissive = LeanTween.value(mirrorMat, color0, color1 * 5, 1f);
        tweenEmissive.setOnUpdate((Color color) => { material.color = color; });
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float,surpriseSentence, 0);

        while (!CheckPlayerEmotion("surprise"))
            yield return null;
        
        NotificationText.Instance.StopDisplayingText(TransitionType.Float);
        yield return new WaitForSecondsRealtime(0.5f);
        
        mirrorPromptPitch.SetValue(gameObject, 1);
        mirrorPromptComplete.Post(gameObject);
        
        tweenEmissive = LeanTween.value(mirrorMat, color1 * 5, color2 * 5, 1f);
        tweenEmissive.setOnUpdate((Color color) => { material.color = color; });
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float,sadSentence, 0);
        
        while (!CheckPlayerEmotion("sadness"))
            yield return null;

        NotificationText.Instance.StopDisplayingText(TransitionType.Float);

        
        mirrorPromptPitch.SetValue(gameObject, 2);
        mirrorPromptComplete.Post(gameObject);
        
        tweenEmissive = LeanTween.value(mirrorMat, color2 * 5, color3 * 5, 1f);
        tweenEmissive.setOnUpdate((Color color) => { material.color = color; });

        //yield return new WaitForSecondsRealtime(2.0f);
        
        // EXIT PUZZLE
        StartCoroutine(PuzzleCompleteCutscene());
    }

    private IEnumerator PuzzleCompleteCutscene()
    {
        MetricScript.LogMetric("Soul Puzzle completed");
        UIVisibilityScript.Instance.HideUI(1.0f);
        BlackBarTransitioner.Instance.Show(1.5f);

        yield return new WaitForSecondsRealtime(1.5f);
        
        PlayerStateScript.Instance.SetMovementActive(false, false);
        
        cutsceneAnimator.SetTrigger(FinishedPuzzle);
        relicGet.Post(gameObject);
        
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
        
        WeatherController.Instance.enabled = true;
        
        if (RelicHolderScript.Instance.CheckForAllRelics() == true)
        {
            PortalScript.Instance.OpenPortal();
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
            ambientMusic.Post(gameObject);
            PlayerStateScript.Instance.SetRaycastState(RaycastState.Enabled);
            PlayerStateScript.Instance.SetMovementActive(true, true);
            BlackBarTransitioner.Instance.Hide(2f);
            UIVisibilityScript.Instance.ShowUI(2.0f);
        }

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
