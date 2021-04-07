using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private bool _paused;

    public GameObject pauseMenu;

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

    private void OpenPauseMenu()
    {
        ChangeCursorState(CursorLockMode.None);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        
        _paused = true;
    }

    private void ClosePauseMenu()
    {
        ChangeCursorState(CursorLockMode.Locked);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        
        _paused = false;
    }
}
