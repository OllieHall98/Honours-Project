using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class PostProcessController : MonoBehaviour
{
    public static PostProcessController Instance;
    
    [SerializeField] private Volume postProcessVolume;
    
    private PostProcessingPreset _postProcessingPreset;
    
    private Vignette _vignette;
    private float _startVignette;
    
    private LensDistortion _lensDistortion;
    private float _startLensDistortion;

    private WhiteBalance _whiteBalance;
    private float _startWhiteBalance;

    public bool enabled;
    
    private bool _coroutineExecuting;
    private float _timer = 0;

    private void Awake()
    {
        Instance = this;
        
        postProcessVolume.profile.TryGet(out _vignette);
        postProcessVolume.profile.TryGet(out _lensDistortion);
        postProcessVolume.profile.TryGet(out _whiteBalance);

        enabled = true;
    }

    public void ChangePostProcessing(PostProcessingPreset preset, float duration)
    {
        if (!enabled) return;

        _postProcessingPreset = preset;

        SetTransitionStartValues();

        if (!_coroutineExecuting)
        {
            StartCoroutine(ChangePostProcessingTransition(duration));
        }
    }
    
    private void SetTransitionStartValues()
    {
        _timer = 0f;

        _startVignette = _vignette.intensity.value;
        _startLensDistortion = _lensDistortion.intensity.value;
        _startWhiteBalance = _whiteBalance.temperature.value;
    }
    
    private IEnumerator ChangePostProcessingTransition(float duration)
    {
        _coroutineExecuting = true;
        
        _timer = 0;

        while (_timer < duration)
        {
            _vignette.intensity.value = Mathf.Lerp(_startVignette, _postProcessingPreset.vignetteIntensity, _timer / duration);
            _lensDistortion.intensity.value = Mathf.Lerp(_startLensDistortion, _postProcessingPreset.lensDistortionIntensity, _timer / duration);
            _whiteBalance.temperature.value = Mathf.Lerp(_startWhiteBalance, _postProcessingPreset.whiteBalanceTemperature, _timer / duration);
         
            _timer += Time.deltaTime;
            yield return null;
        }
        
        _coroutineExecuting = false;
    }
}
