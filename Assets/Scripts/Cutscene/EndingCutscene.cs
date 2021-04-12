using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Weather;

public class EndingCutscene : MonoBehaviour
{
    public Image fader;

    [SerializeField] private WeatherTypeData teleportWeather;

    [SerializeField] private AK.Wwise.Event teleportSound;
    
    public void EndGame()
    {
        StartCoroutine(End());
    }

    private IEnumerator End()
    {
        teleportSound.Post(gameObject);
        
        PlayerStateScript.Instance.SetMovementActive(false, true);
        UIVisibilityScript.Instance.HideUI(1f);
        
        WeatherController.Instance.ChangeWeather(teleportWeather, 4.0f);
        
        var tweenOpacity = LeanTween.value(fader.gameObject, 0, 1, 3f);
        tweenOpacity.setOnUpdate((float opacity) => { fader.color = new Color(1,1,1, opacity); });

        yield return new WaitForSecondsRealtime(5.0f);

        SceneManager.LoadScene("Main Menu");
    }
}
