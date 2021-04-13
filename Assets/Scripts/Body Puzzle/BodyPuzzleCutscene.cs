using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Weather;

public class BodyPuzzleCutscene : MonoBehaviour
{
    private Animator _cutsceneAnimator;
    private static readonly int StartCutscene1 = Animator.StringToHash("StartCutscene");

    [SerializeField] private AK.Wwise.Event relicAcquired;
    [SerializeField] private AK.Wwise.Event mazeDiscovery;
    
    [SerializeField] private GameObject bodyBeacon;
    [SerializeField] private Image fader;
    
    private void Awake()
    {
        _cutsceneAnimator = GetComponent<Animator>();
    }
    

    public void StartCutscene()
    {
        StartCoroutine(Cutscene());
    }

    private IEnumerator Cutscene()
    {
        PlayerStateScript.Instance.SetMovementActive(false, false);
        UIVisibilityScript.Instance.HideUI(0.5f);
        BlackBarTransitioner.Instance.Show(1.5f);
        
        yield return new WaitForSecondsRealtime(0.2f);
        
        AkSoundEngine.StopAll();
        
        mazeDiscovery.Post(gameObject);
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        MazePickup.Instance.RemovePaper();
        
        _cutsceneAnimator.SetTrigger(StartCutscene1);

        yield return new WaitForSecondsRealtime(6f);
        
        relicAcquired.Post(gameObject);
        
        yield return new WaitForSecondsRealtime(4f);
        
        NotificationText.Instance.DisplayMessage(TransitionType.Float, "<color=red>Body Relic</color> obtained.", 3.0f);
        
        yield return new WaitForSecondsRealtime(3f);
        
        var tweenOpacity = LeanTween.value(fader.gameObject, 0, 1, 2f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        
        yield return new WaitForSecondsRealtime(2.5f);
        
        tweenOpacity = LeanTween.value(fader.gameObject, 1, 0, 2f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        
        bodyBeacon.SetActive(false);
        RelicHolderScript.Instance.EnableRelic(RelicHolderScript.Instance.bodyRelic);
        RelicUIManager.Instance.EnableRelicUI(RelicUIManager.Instance.bodyRelic);
        
        PlayerStateScript.Instance.gameObject.transform.position = PlayerStateScript.Instance.playerStartTransform.position;
        
        if (RelicHolderScript.Instance.CheckForAllRelics() == true)
        {
            PortalScript.Instance.OpenPortal();
        }
        else
        {
            yield return new WaitForSecondsRealtime(3f);
            PlayerStateScript.Instance.SetMovementActive(true, true);
            BlackBarTransitioner.Instance.Hide(2f);
            UIVisibilityScript.Instance.ShowUI(2.0f);
        }
        
    }
}
