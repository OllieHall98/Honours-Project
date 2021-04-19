using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class RandomShadowDisabler : MonoBehaviour
{
    private void Awake()
    {
        int i = Random.Range(0, 1);

        if (i != 0) return;
        
        var renderers = GetComponentsInChildren<Renderer>();
            
        foreach (var r in renderers)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
        }
    }

}
