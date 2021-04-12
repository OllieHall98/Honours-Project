using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Affective;

public enum Emotion
{
    Neutral,
    Joy,
    Sadness,
    Fear,
    Anger,
    Disgust,
    Surprise,
    None
}

public class MazeGateScript : ObjectState
{
    [SerializeField] private Emotion currentEmotion = Emotion.None; 
    [SerializeField] private Emotion desiredEmotion;

    [SerializeField] private GameObject soundEmitter;
    [SerializeField] private AK.Wwise.Event gateOpenSound;

    private Animator _gateAnimator;

    private GameObject _player;
    [SerializeField] private BoxCollider triggerCollider;
    private static readonly int Open = Animator.StringToHash("Open");

    private bool _opened = false;
    
    private new void Awake()
    {
        base.Awake();
        _gateAnimator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        _opened = false;
        _player = PlayerStateScript.Instance.gameObject;
    }

    private void CheckIfDesiredEmotion(Emotion emotion)
    {
        if (emotion != desiredEmotion) return;

        if (_opened) return;
        
        if (!triggerCollider.bounds.Contains(_player.transform.position)) return;

        _opened = true;
        gateOpenSound.Post(soundEmitter);
        _gateAnimator.SetTrigger(Open);
    }
    
    public override void Neutral_State() => CheckIfDesiredEmotion(Emotion.Neutral);
    public override void Joy_State() => CheckIfDesiredEmotion(Emotion.Joy);
    public override void Sadness_State() => CheckIfDesiredEmotion(Emotion.Sadness);
    public override void Fear_State() => CheckIfDesiredEmotion(Emotion.Fear);
    public override void Anger_State() => CheckIfDesiredEmotion(Emotion.Anger);
    public override void Disgust_State() => CheckIfDesiredEmotion(Emotion.Disgust);
    public override void Surprise_State() => CheckIfDesiredEmotion(Emotion.Surprise);
}
