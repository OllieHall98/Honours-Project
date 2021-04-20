using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] private CanvasGroup loadingScreen;
    [SerializeField] private TMPro.TextMeshProUGUI loadingText;
    

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1;
        
        loadingText.enabled = false;
        loadingScreen.alpha = 1;
    }

    private void Start()
    {
        var tweenOpacity = LeanTween.value(loadingScreen.gameObject, 1, 0, 0.5f);
        tweenOpacity.setOnUpdate((float opacity) => { loadingScreen.alpha = opacity; });
    }

    public void StartGame()
    {
        StartCoroutine(StartLoadingGame());
    }

    private IEnumerator StartLoadingGame()
    {
        loadingText.enabled = true;
        
        var tweenOpacity = LeanTween.value(loadingScreen.gameObject, 0, 1, 1f);
        tweenOpacity.setOnUpdate((float opacity) => { loadingScreen.alpha = opacity; });

        yield return new WaitForSecondsRealtime(1.2f);

        SceneManager.LoadScene("SampleScene");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
