using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private bool _paused;

    public GameObject pauseMenu;

    [SerializeField] private AK.Wwise.Event pause, resume;

    private void Start()
    {
        _paused = false;
    }

    private void Update()
    {
        if (!Input.GetButtonDown("Cancel")) return;
        
        if (!_paused)
            OpenPauseMenu();
        else
            ClosePauseMenu();
    }

    private static void ChangeCursorState(CursorLockMode lockMode)
    {
        Cursor.lockState = lockMode;
        Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
    }

    public void OpenPauseMenu()
    {
        pause.Post(gameObject);
        
        ChangeCursorState(CursorLockMode.None);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        
        _paused = true;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ClosePauseMenu()
    {
        resume.Post(gameObject);
        
        ChangeCursorState(CursorLockMode.Locked);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        
        _paused = false;
    }
}
