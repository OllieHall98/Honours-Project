using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Weather;

public class IntroCinematicScript : MonoBehaviour
{
    public string gameTitle;

    public AK.Wwise.Event music;
    public AK.Wwise.Event gameplayStartmusic;
    
    public GameObject blackBars;
    public Image fader;

    public WeatherTypeData weatherNight;
    public WeatherTypeData weatherSunrise;
    public WeatherTypeData weatherDay;

    [SerializeField] private float fadeTime;

    public Animator cutsceneAnimator;
    private static readonly int StartAnim = Animator.StringToHash("StartAnim");

    public bool skipCutscene = false;
    
    private void Start()
    {
        if (skipCutscene)
        {
            WeatherController.Instance.startWeather = weatherDay;
            GetComponent<Camera>().enabled = false;
            
            UIVisibilityScript.Instance.ShowUI(2f);
            
            return;
        }

        WeatherController.Instance.startWeather = weatherNight;
        WeatherController.Instance.ResetWeather();

        StartCoroutine(Cutscene());
    }

    private IEnumerator Cutscene()
    {
        PlayerStateScript.Instance.playerCamera.enabled = false;
        
        cutsceneAnimator.SetTrigger(StartAnim);
        UIVisibilityScript.Instance.HideUIInstant();
        PlayerStateScript.Instance.SetMovementActive(false, false);

        PortalScript.Instance.Enable();
        
        music.Post(gameObject);

        fader.color = new Color(0, 0, 0, 1);

        blackBars.SetActive(true);
        BlackBarTransitioner.Instance.Show(0.1f);
        
        yield return new WaitForSecondsRealtime(1f);
        var tweenOpacity = LeanTween.value(fader.gameObject, 1, 0, fadeTime);
            tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(0,0,0, opacity); });
        yield return new WaitForSecondsRealtime(7f);
        WeatherController.Instance.ChangeWeather(weatherDay, 20.0f);
        yield return new WaitForSecondsRealtime(2f);
        NotificationText.Instance.DisplayMessage(TransitionType.Fade, gameTitle, 10.0f);
        yield return new WaitForSecondsRealtime(10f);
        //WeatherController.Instance.ChangeWeather(weatherDay, 12.0f);
        yield return new WaitForSecondsRealtime(10f);
        yield return new WaitForSecondsRealtime(2f);
        BlackBarTransitioner.Instance.Hide(3f);
        yield return new WaitForSecondsRealtime(2.5f);
        tweenOpacity = LeanTween.value(fader.gameObject, 0, 1, 2f);
            tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
            // After cutscene ended
        yield return new WaitForSecondsRealtime(5.5f);
        

        tweenOpacity = LeanTween.value(fader.gameObject, 1, 0, 2f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });
        PlayerStateScript.Instance.playerCamera.enabled = true;
        GetComponent<Camera>().enabled = false;
        PortalScript.Instance.Disable();
        
        yield return new WaitForSecondsRealtime(1f);
        UIVisibilityScript.Instance.ShowUI(2f);
        
        yield return new WaitForSecondsRealtime(2f);
        
        PlayerStateScript.Instance.SetMovementActive(true, true);

        yield return new WaitForSecondsRealtime(8f);
        
        gameplayStartmusic.Post(gameObject);
    }
}
