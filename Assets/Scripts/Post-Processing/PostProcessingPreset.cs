using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "PostProcessTypeData", menuName = "ScriptableObjects/PostProcessType", order = 1)]
public class PostProcessingPreset : ScriptableObject
{
    [Header("Between -20 and 20")]
    public float whiteBalanceTemperature;
    
    [Header("Between 0 and 0.3")]
    public float vignetteIntensity;

    [Header("Between 0 and 0.5")]
    public float lensDistortionIntensity;

}
