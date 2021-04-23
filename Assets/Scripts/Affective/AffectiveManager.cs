using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Affective;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Events;

public class AffectiveManager : MonoBehaviour
{
    public enum AffectiveMode
    {
        Reinforce,
        Support
    }

    public AffectiveMode currentAffectiveMode;
    
    
    public static AffectiveManager Instance;
    
    #region events

    private UnityEvent _currentEvent;
    
    private UnityEvent _neutralEvent;
    private UnityEvent _joyEvent;
    private UnityEvent _sadnessEvent;
    private UnityEvent _fearEvent;
    private UnityEvent _disgustEvent;
    private UnityEvent _angerEvent;
    private UnityEvent _surpriseEvent;
    #endregion

    private Queue<string> _emotionList;
    private string _currentEmotion;

    public TextMeshProUGUI emotionText;

    private void Awake()
    {
        Instance = this;
        
        _emotionList = new Queue<string>();
        
        _currentEmotion = "neutral";
    }

    private void Start()
    {
        InitialiseEvents();
    }

    private void InitialiseEvents()
    {
        _neutralEvent    = new UnityEvent();
        _joyEvent        = new UnityEvent();
        _sadnessEvent    = new UnityEvent();
        _fearEvent       = new UnityEvent();
        _disgustEvent    = new UnityEvent();
        _angerEvent      = new UnityEvent();
        _surpriseEvent   = new UnityEvent();

        foreach (var obj in GameObject.FindGameObjectsWithTag("Affective"))
        {
            foreach (var objectState in obj.GetComponents<ObjectState>())
            {
                _neutralEvent.AddListener(objectState.ANeutral);
                _joyEvent.AddListener(objectState.AJoy);
                _sadnessEvent.AddListener(objectState.ASadness);
                _fearEvent.AddListener(objectState.AFear);
                _disgustEvent.AddListener(objectState.ADisgust);
                _angerEvent.AddListener(objectState.AAnger);
                _surpriseEvent.AddListener(objectState.ASurprise);
            }
            
            // _neutralEvent.AddListener(obj.GetComponents<ObjectState>().ANeutral);
            // _joyEvent.AddListener(obj.GetComponent<ObjectState>().AJoy);
            // _sadnessEvent.AddListener(obj.GetComponent<ObjectState>().ASadness);
            // _fearEvent.AddListener(obj.GetComponent<ObjectState>().AFear);
            // _disgustEvent.AddListener(obj.GetComponent<ObjectState>().ADisgust);
            // _angerEvent.AddListener(obj.GetComponent<ObjectState>().AAnger);
            // _surpriseEvent.AddListener(obj.GetComponent<ObjectState>().ASurprise);
        }
    }

    public string GetCurrentEmotion() => _currentEmotion;

    // private void RaiseEvent(UnityEvent e)
    // {
    //     //CancelInvoke("_currentEvent");
    //     e.Invoke();
    // }
    

    public void SetCurrentEmotion(string emotion)
    {
        //Debug.Log(emotion);
        
        if (emotion == "N/A" || emotion == "")
        { 
            Debug.Log("No Face Detected");
            emotionText.text = "No Face Detected";
            return;
        }
            
        string strongestEmotion = CalculateStrongestEmotion(emotion);

        if (strongestEmotion == null || strongestEmotion == _currentEmotion)
            return;
        
        emotionText.text = "Current Emotion: " + char.ToUpper(strongestEmotion[0]) + strongestEmotion.Substring(1);
            
        //NotificationText.Instance.DisplayMessage("Current Emotion: " + char.ToUpper(strongestEmotion[0]) + strongestEmotion.Substring(1), 1f);
        
        _currentEmotion = strongestEmotion;

        _currentEvent = _currentEmotion switch
        {
            "neutral" => _neutralEvent,
            "surprise" => _surpriseEvent,
            "sadness" => _sadnessEvent,
            "fear" => _fearEvent,
            "disgust" => _disgustEvent,
            "anger" => _angerEvent,
            "joy" => _joyEvent,
            _ => _currentEvent
        };
        
        MetricScript.Instance.LogMetric("Emotion changed to " + _currentEmotion);
        
        _currentEvent.Invoke();
    }

    private string CalculateStrongestEmotion(string emotion)
    {
        if (_emotionList.Count > 6) _emotionList.Dequeue();
        _emotionList.Enqueue(emotion);
        
        var groups = _emotionList.ToArray().GroupBy(s => s);

        var highestGroupCount = 0;
        string strongestEmotion = null;

        foreach (var group in groups)
        {
            if (@group.Count() <= highestGroupCount) continue;
                
            highestGroupCount = @group.Count();
            strongestEmotion = @group.Key;
        }

        return strongestEmotion;
    }
}