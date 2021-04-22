using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Affective;
using Random = UnityEngine.Random;

public class AffectiveButterfly : ObjectState
{
    private GameObject _player;
    private Vector3 _offset;

    private GameObject _butterfly;
    private Animator _butterflyAnimator;

    private Camera _playerCamera;
    
    private void Start()
    {
        _player = GameObject.Find("Player");
        _butterfly = transform.GetChild(0).gameObject;
        _butterflyAnimator = GetComponent<Animator>();

        _playerCamera = Camera.main;

        _offset = new Vector3(0, 2, 0);
        
        _butterfly.SetActive(false);
    }

    private void Update()
    {
        transform.position = _player.transform.position + _offset;
    }

    private bool IsVisible()
    {
        if (_playerCamera is null) return false;
        
        var screenPoint = _playerCamera.WorldToViewportPoint(_butterfly.transform.position);

        return (
            screenPoint.x > -0.25 &&
            screenPoint.x < 1.25 &&
            screenPoint.y > -0.25 &&
            screenPoint.y < 1.25 &&
            screenPoint.z > -0.25);

    }

    public override void Neutral_State()
    {
        
    }

    public override void Joy_State()
    {
        StartCoroutine(ShowButterfly());
    }

    public override void Sadness_State()
    {
        StartCoroutine(HideButterfly());
    }
    public override void Fear_State()     
    {
        StartCoroutine(HideButterfly());
    }

    public override void Disgust_State()
    {
        StartCoroutine(HideButterfly());
    }

    public override void Anger_State()
    {
        StartCoroutine(HideButterfly());
    }

    public override void Surprise_State()
    {
        StartCoroutine(HideButterfly());
    }
    
    
    private IEnumerator ShowButterfly()
    {
       // int range = Random.Range(0, 1);
       // if (range == 0) yield break;

       while (IsVisible())
       {
           yield return new WaitForSecondsRealtime(0.25f);
       }
       
       _butterfly.SetActive(true);
       
        // if (!IsVisible()) 
        //     _butterfly.SetActive(true);
        // else
        // {
        //     yield return new WaitForSecondsRealtime(0.5f);
        //     if (!IsVisible()) 
        //         _butterfly.SetActive(true);
        //     else
        //     {
        //         yield return new WaitForSecondsRealtime(0.5f);
        //         if (!IsVisible()) 
        //             _butterfly.SetActive(true);
        //     }
        // }
    }

    private IEnumerator HideButterfly()
    {
        // if (!IsVisible()) 
        //     _butterfly.SetActive(false);
        // else
        // {
        //     yield return new WaitForSecondsRealtime(0.5f);
        //     if (!IsVisible()) 
        //         _butterfly.SetActive(false);
        //     else
        //     {
        //         yield return new WaitForSecondsRealtime(0.5f);
        //         if (!IsVisible()) 
        //             _butterfly.SetActive(false);
        //     }
        // }
        
        while (IsVisible())
        {
            yield return new WaitForSecondsRealtime(0.25f);
        }
       
        _butterfly.SetActive(false);

    }

}
