using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Weather;

public class EndingCutscene : MonoBehaviour
{
    public Image fader;

    public TextMeshProUGUI endingText;

    [SerializeField] private WeatherTypeData teleportWeather;

    [SerializeField] private AK.Wwise.Event teleportSound;

    [SerializeField] private Animator endingAnimator;
    [SerializeField] private Animator portalAnimator;

    [SerializeField] private AK.Wwise.Event endingMusic;
    private static readonly int ClosePortal = Animator.StringToHash("ClosePortal");
    private static readonly int Start = Animator.StringToHash("Start");

    public void EndGame()
    {
        StartCoroutine(End());
    }

    private IEnumerator End()
    {
        MetricScript.LogMetric("End of game");
        
        teleportSound.Post(gameObject);
        
        PlayerStateScript.Instance.SetMovementActive(false, true);
        UIVisibilityScript.Instance.HideUI(1f);
        BlackBarTransitioner.Instance.Show(1.5f);
        
        WeatherController.Instance.ChangeWeather(teleportWeather, 25.0f);
        
        yield return new WaitForSecondsRealtime(2.0f);

        endingMusic.Post(gameObject);

        endingAnimator.SetTrigger(Start);
        
        portalAnimator.SetTrigger(ClosePortal);
        
        yield return new WaitForSecondsRealtime(29.0f);
        
        var tweenTextOpacity = LeanTween.value(endingText.gameObject, 0, 1, 3f);
        tweenTextOpacity.setOnUpdate((float opacity) => { endingText.color = new Color(1,1,1, opacity); });

        yield return new WaitForSecondsRealtime(2.0f);
        
        var tweenOpacity = LeanTween.value(fader.gameObject, 0, 1, 3f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(0,0,0, opacity); });
        
        yield return new WaitForSecondsRealtime(4.0f);
        
        SceneManager.LoadScene("Main Menu");
    }
}
