using System.Collections;
using System.Collections.Generic;
using Affective;
using UnityEngine;

public class AffectiveAudio : ObjectState
{
    [SerializeField] private AK.Wwise.RTPC environmentVolume;

    private bool _coroutineExecuting;
    private float _timer;

    private float startEnvironmentVolume;
    private float targetEnvironmentValue;
    
    public override void Neutral_State()
    {
        StartRtpcTransition(80);
    }

    public override void Joy_State()
    {
        StartRtpcTransition(100);
    }

    public override void Sadness_State()
    {
        StartRtpcTransition(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce ? 40 : 80);
    }
    public override void Fear_State()     
    {
        StartRtpcTransition(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce ? 0 : 80);
    }

    public override void Disgust_State()
    {
        environmentVolume.SetGlobalValue(100);
    }
    
    public override void Anger_State()
    {
        StartRtpcTransition(
            AffectiveManager.Instance.currentAffectiveMode == AffectiveManager.AffectiveMode.Reinforce ? 50 : 80);
    }

    public override void Surprise_State()
    {
        StartRtpcTransition(100);
    }

    private void StartRtpcTransition(float newValue)
    {
        startEnvironmentVolume = environmentVolume.GetGlobalValue();
        targetEnvironmentValue = newValue;

        StartCoroutine(ChangeRtpcValues(4f));
    }
    
    private IEnumerator ChangeRtpcValues(float duration)
        {
            _coroutineExecuting = true;
            
            _timer = 0;

            float currentEnvironmentVolume = startEnvironmentVolume;
            
            while (_timer < duration)
            {
                currentEnvironmentVolume = Mathf.Lerp(startEnvironmentVolume, targetEnvironmentValue,
                    _timer / duration);
                
                environmentVolume.SetGlobalValue(currentEnvironmentVolume);
                
                _timer += Time.deltaTime;
                yield return null;
            }
            
            _coroutineExecuting = false;
        }

}
