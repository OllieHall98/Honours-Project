using System;
using System.Collections;
using UI;
using UnityEngine.UI;
using UnityEngine;

public class OpenChest : MonoBehaviour
{
    public static OpenChest Instance;

    [SerializeField] private MoodReceptorManager _receptorManager;

    [SerializeField] private Animator mindPuzzleAnimator;
    private static readonly int Chest = Animator.StringToHash("OpenChest");
    private static readonly int Complete = Animator.StringToHash("Complete");
    [SerializeField] private Transform targetTransform;

    
    
    [SerializeField] private GameObject mindBeacon;

    public AK.Wwise.Event openChestAudio;
    [SerializeField] private AK.Wwise.Event startMusic;
    [SerializeField] private AK.Wwise.Event teleportSound;
    [SerializeField] private AK.Wwise.Event RelicGet;
    
    public Image fader;
    
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
        //if(!_coroutineExecuting) StartCoroutine(MovePlayerToPosition());
        
        //mindPuzzleAnimator.SetTrigger(Chest);
        StartCoroutine(StartPuzzleCutscene());
    }
    
    IEnumerator StartPuzzleCutscene()
    {
        BlackBarTransitioner.Instance.Show(0.4f);
        UIVisibilityScript.Instance.HideUI(0.25f);
        
        yield return new WaitForSecondsRealtime(1.0f);
        
        AkSoundEngine.StopAll();
        MovePlayerToPosition();
        
        openChestAudio.Post(gameObject);
        mindPuzzleAnimator.SetTrigger(Chest);
        
        ReticleManager.Instance.HideReticle();

        yield return new WaitForSecondsRealtime(1.0f);
        
        _receptorManager.StartReceptorAudio();
        
        yield return new WaitForSecondsRealtime(3.0f);
        
        conveyanceCube.SetActive(true);
        _receptorManager.SetConveyanceCube();
        yield return new WaitForSecondsRealtime(2.75f);
        
        EndCutscene();
    }

    public IEnumerator EndPuzzleCutscene()
    {
        PlayerStateScript.Instance.SetMovementActive(false, false);
        BlackBarTransitioner.Instance.Show(1f);
        UIVisibilityScript.Instance.HideUI(1.0f);
        RelicGet.Post(gameObject);
        yield return new WaitForSecondsRealtime(2f);
        ConveyanceCubeScript.Instance.StopUsingCube();
        mindPuzzleAnimator.SetTrigger(Complete);
        yield return new WaitForSecondsRealtime(2.5f);
        var tweenOpacity = LeanTween.value(fader.gameObject, 0, 1, 3f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        yield return new WaitForSecondsRealtime(2.5f);
        _receptorManager.StopReceptorAudio();
        StartCoroutine(EndPuzzleCutscene2());
    }

    private IEnumerator EndPuzzleCutscene2()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        teleportSound.Post(gameObject);
        var tweenOpacity = LeanTween.value(fader.gameObject, 1, 0, 0.5f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        yield return new WaitForSecondsRealtime(2.5f);
        NotificationText.Instance.DisplayMessage(TransitionType.Float, "<color=#40E0D0>Mind Relic</color> obtained.", 4.0f);
        yield return new WaitForSecondsRealtime(5.0f);
        var tweenOpacity1 = LeanTween.value(fader.gameObject, 0, 1, 1f);
        tweenOpacity1.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        yield return new WaitForSecondsRealtime(2f);
        tweenOpacity = LeanTween.value(fader.gameObject, 1, 0, 0.5f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        mindBeacon.SetActive(false);
        RelicHolderScript.Instance.EnableRelic(RelicHolderScript.Instance.mindRelic);
        RelicUIManager.Instance.EnableRelicUI(RelicUIManager.Instance.mindRelic);
        _player.transform.position = PlayerStateScript.Instance.playerStartTransform.position;
        PlayerStateScript.Instance.SetMovementActive(true, true);
        PlayerStateScript.Instance.SetRaycastState(RaycastState.Enabled);
        yield return new WaitForSecondsRealtime(1f);
        BlackBarTransitioner.Instance.Hide(2f);
        UIVisibilityScript.Instance.ShowUI(2.0f);
    }

    private void EndCutscene()
    {
        BlackBarTransitioner.Instance.Hide(1f);
        
        ConveyanceCubeScript.Instance.Pickup();
        
        _receptorManager.AddReceptorsToDictionary();
        
        UIVisibilityScript.Instance.ShowUI(0.5f);

        startMusic.Post(gameObject);
    }

    //private bool _coroutineExecuting;
    

    // private IEnumerator MovePlayerToPosition()
    // {
    //     _coroutineExecuting = true;
    //     
    //     PlayerStateScript.Instance.SetMovementActive(false, true);
    //
    //     
    //     float timer = 0;
    //     
    //     var targetPosition = targetTransform.position;
    //     var startPosition = _player.transform.position;
    //     var startForward = _cameraPivot.transform.localEulerAngles;
    //
    //     var modifiedTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);
    //
    //     Vector3 targetForward = new Vector3(0, 0, 0);
    //     
    //     while(timer < lerpDuration)
    //     {
    //         _player.transform.position = Vector3.Lerp(startPosition, modifiedTargetPosition, timer / lerpDuration);
    //         _cameraPivot.transform.localEulerAngles = Vector3.Lerp(startForward, targetForward, timer / lerpDuration);
    //         timer += Time.deltaTime;
    //         
    //         yield return null;
    //     }
    //
    //     _player.transform.position = modifiedTargetPosition;
    //     _cameraPivot.transform.localEulerAngles = targetForward;
    //
    //     _coroutineExecuting = false;
    // }

    void MovePlayerToPosition()
    {
        PlayerStateScript.Instance.SetMovementActive(false, true);
        
        var targetPosition = targetTransform.position;
        var startPosition = _player.transform.position;
        
        var modifiedTargetPosition = new Vector3(targetPosition.x, startPosition.y, targetPosition.z);
        Vector3 targetForward = new Vector3(0, 0, 0);
        
        _player.transform.position = modifiedTargetPosition;
        _cameraPivot.transform.localEulerAngles = targetForward;
    }
    
    
}
