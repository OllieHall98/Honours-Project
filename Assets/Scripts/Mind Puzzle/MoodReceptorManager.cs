using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoodReceptorManager : MonoBehaviour
{
    [SerializeField] private GameObject cube;
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
            if (value > 55 || value < 45)
                allReceptorsCorrect = false;
        }

        if (allReceptorsCorrect)
        {
            cube.SetActive(false);
            _puzzleCompleted = true;
        }
       
        yield return new WaitForSecondsRealtime(0.5f);
        
        _coroutineExecuting = false;
    }
}
