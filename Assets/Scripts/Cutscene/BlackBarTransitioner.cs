using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlackBarTransitioner : MonoBehaviour
{
    public static BlackBarTransitioner Instance;
    
    private GameObject _bar1, _bar2;
    private RectTransform _bar1Rect, _bar2Rect;

    [SerializeField] private float yOffset;

    private void Awake()
    {
        Instance = this;
        
        _bar1 = transform.GetChild(0).gameObject;
        _bar2 = transform.GetChild(1).gameObject;

        _bar1Rect = _bar1.GetComponent<RectTransform>();
        _bar2Rect = _bar2.GetComponent<RectTransform>();
    }
    

    public void Show(float speed)
    {
        var bar1Anchored = _bar1Rect.anchoredPosition;
        var tweenPosition1 = LeanTween.value(_bar1, bar1Anchored, bar1Anchored - new Vector2(0, yOffset), speed);
        
        var bar2Anchored = _bar2Rect.anchoredPosition;
        var tweenPosition2 = LeanTween.value(_bar2, bar2Anchored, bar2Anchored + new Vector2(0, yOffset), speed);
        
        tweenPosition1.setOnUpdate((Vector2 val) => { _bar1Rect.anchoredPosition = val; });
        tweenPosition2.setOnUpdate((Vector2 val) => { _bar2Rect.anchoredPosition = val; });
    }
    
    public void Hide(float speed)
    {
        var bar1Anchored = _bar1Rect.anchoredPosition;
        var tweenPosition1 = LeanTween.value(_bar1, bar1Anchored, bar1Anchored + new Vector2(0, yOffset), speed);
        
        var bar2Anchored = _bar2Rect.anchoredPosition;
        var tweenPosition2 = LeanTween.value(_bar2, bar2Anchored, bar2Anchored - new Vector2(0, yOffset), speed);
        
        tweenPosition1.setOnUpdate((Vector2 val) => { _bar1Rect.anchoredPosition = val; });
        tweenPosition2.setOnUpdate((Vector2 val) => { _bar2Rect.anchoredPosition = val; });
    }
    
}
