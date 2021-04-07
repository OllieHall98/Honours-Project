using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodReceptorManager : MonoBehaviour
{
    private MoodReceptorScript[] _moodReceptors;

    private bool _coroutineExecuting = false;

    private bool _puzzleCompleted;

    private ConveyanceCubeScript _conveyanceCubeScript;
    private GameObject _conveyanceCube;
    
    private void Start()
    {
        _conveyanceCubeScript = ConveyanceCubeScript.Instance;

        _moodReceptors = GetComponentsInChildren<MoodReceptorScript>();
    }

    public void SetConveyanceCube()
    {
        _conveyanceCubeScript = ConveyanceCubeScript.Instance;
    }
    
    private void Update()
    {
        if (_conveyanceCubeScript == null) return;
        
        if (!_conveyanceCubeScript.isHeld || _puzzleCompleted) return;
        
        if(!_coroutineExecuting) StartCoroutine(CheckReceptors());
    }

    public void StartReceptorAudio()
    {
        foreach (var moodReceptor in _moodReceptors)
        {
            moodReceptor.StartAudio();
        }
    }
    
    public void StopReceptorAudio()
    {
        foreach (var moodReceptor in _moodReceptors)
        {
            moodReceptor.StopAudio();
        }
    }

    public void AddReceptorsToDictionary()
    {
        foreach (var moodReceptor in _moodReceptors)
        {
            moodReceptor.AddToDictionary();
        }
    }

    IEnumerator CheckReceptors()
    {
        _coroutineExecuting = true;

        var allReceptorsCorrect = true;
        
        foreach (var moodReceptor in _moodReceptors)
        {
            float value = moodReceptor.value;
            if (value > 57 || value < 43)
                allReceptorsCorrect = false;
        }

        if (allReceptorsCorrect)
        {
            StartCoroutine(OpenChest.Instance.EndPuzzleCutscene());
            
            foreach (var moodReceptor in _moodReceptors)
            {
                moodReceptor.active = false;
            }
            
            _puzzleCompleted = true;
        }
       
        yield return new WaitForSecondsRealtime(0.5f);
        
        _coroutineExecuting = false;
    }
}
