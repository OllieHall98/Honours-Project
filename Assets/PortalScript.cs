using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : MonoBehaviour
{
    public static PortalScript Instance;
    
    [SerializeField] private GameObject portalLight;

    private void Awake()
    {
        Instance = this;
    }

    public void Enable()
    {
        portalLight.SetActive(true);
    }
    
    public void Disable()
    {
        portalLight.SetActive(false);
    }
}
