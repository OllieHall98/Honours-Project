using System;
using System.Collections;
using UnityEngine;

public class MoodReceptorScript : MonoBehaviour
{
    private Material _receptorMaterial;
    private ParticleSystem _particleSystem;
    private ParticleSystemRenderer _particleSystemRenderer;
    [SerializeField] private AnimationCurve emissionCurve = AnimationCurve.EaseInOut(0f,0f,1f,0f);
    [SerializeField] private Gradient colorGradient;

    private Light _light;
    
#region equilibrium values
    private float _minValue, _maxValue;
    public float value;
#endregion

    public bool executing = false;
    
    private float _emissionIntensity;
    private static readonly int MaterialColor = Shader.PropertyToID("_EmissiveColor");
    
    private void Start()
    {
        ConveyanceCubeScript.Instance.MoodReceptorDictionary.Add(transform, this);
        
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _particleSystemRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
        
        _receptorMaterial = GetComponent<Renderer>().material;
        
        _particleSystemRenderer.trailMaterial = _receptorMaterial;

        _light = GetComponentInChildren<Light>();
        
        _minValue = 0;
        _maxValue = 100;
        
        UpdateColors();
    }

    private Color _color;

    public void DetermineValueChange(CubeState state)
    {
        float amount = state switch
        {
            CubeState.Positive when value < _maxValue => 8,
            CubeState.Negative when value > _minValue => -8,
            _ => 0
        };

        value += amount * Time.deltaTime;
        UpdateColors();
    }

    private void UpdateColors()
    {
        _color = colorGradient.Evaluate(value / 100);
        _emissionIntensity = emissionCurve.Evaluate(value) * 10;

        _particleSystemRenderer.material.SetColor(MaterialColor, _color * _emissionIntensity);
        _receptorMaterial.SetColor(MaterialColor, _color * _emissionIntensity);
        _light.color = colorGradient.Evaluate(value / 100);
    }
}
