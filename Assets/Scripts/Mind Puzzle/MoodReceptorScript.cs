using System;
using System.Collections;
using UnityEngine;

public class MoodReceptorScript : MonoBehaviour
{
    private Material _receptorMaterial;
    private ParticleSystem _particleSystem;
    private ParticleSystemRenderer _particleSystemRenderer;
    private ParticleSystem.MainModule _particleSystemMain;
    private ParticleSystem.ShapeModule _particleSystemShape;
    private ParticleSystem.VelocityOverLifetimeModule _particleSystemVelocity;
    [SerializeField] private AnimationCurve emissionCurve = AnimationCurve.EaseInOut(0f,0f,1f,0f);
    [SerializeField] private AnimationCurve particleRadiusCurve;
    [SerializeField] private AnimationCurve particleRadialCurve;
    [SerializeField] private Gradient colorGradient;

    [SerializeField] private AK.Wwise.Event humAudioEvent;
    [SerializeField] private AK.Wwise.Event humStopAudioEvent;
    [SerializeField] private AK.Wwise.RTPC humAudioValue;

    public bool active;
    
    //private Light _light;
    
#region equilibrium values
    private float _minValue, _maxValue;
    public float value;
#endregion

    public bool executing = false;
    
    private float _emissionIntensity;
    private static readonly int MaterialColor = Shader.PropertyToID("_EmissiveColor");
    
    private void Start()
    {
        active = true;
        
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _particleSystemRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
        _particleSystemShape = _particleSystem.shape;
        _particleSystemMain = _particleSystem.main;
        _particleSystemVelocity = _particleSystem.velocityOverLifetime;
        
        _receptorMaterial = GetComponent<Renderer>().material;
        
        _particleSystemRenderer.trailMaterial = _receptorMaterial;

        //_light = GetComponentInChildren<Light>();
        
        _minValue = 0;
        _maxValue = 100;
        
        UpdateAudioVisuals();
    }

    public void StartAudio()
    {
        humAudioEvent.Post(gameObject);
    }
    
    public void StopAudio()
    {
        humStopAudioEvent.Post(gameObject);
    }

    public void AddToDictionary()
    {
        ConveyanceCubeScript.Instance.MoodReceptorDictionary.Add(transform, this);
    }

    private Color _color;

    public void DetermineValueChange(CubeState state)
    {
        if (!active) return;
        
        float amount = state switch
        {
            CubeState.Positive when value < _maxValue => 8,
            CubeState.Negative when value > _minValue => -8,
            _ => 0
        };

        value += amount * Time.deltaTime;
        UpdateAudioVisuals();
    }

    private void UpdateAudioVisuals()
    {
        _color = colorGradient.Evaluate(value / 100);
        _emissionIntensity = emissionCurve.Evaluate(value) * 10;

        _particleSystemVelocity.radial = particleRadialCurve.Evaluate(value / 100);
        
        _particleSystemShape.radius = 0.55f + particleRadiusCurve.Evaluate(value / 100);
        _particleSystemVelocity.speedModifier = 2 + particleRadiusCurve.Evaluate(value / 50);
        
        _particleSystemRenderer.material.SetColor(MaterialColor, _color * _emissionIntensity);
        _receptorMaterial.SetColor(MaterialColor, _color * _emissionIntensity);
        //_light.color = colorGradient.Evaluate(value / 100);

        humAudioValue.SetValue(gameObject, value);
    }
}
