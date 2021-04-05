using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleManager : MonoBehaviour
{
    public static ReticleManager Instance;
    
    public GameObject reticle;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowReticle() => reticle.SetActive(true);
    public void HideReticle() => reticle.SetActive(false);

}
