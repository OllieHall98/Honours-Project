using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicHolderScript : MonoBehaviour
{
    public static RelicHolderScript Instance;

    public GameObject mindRelic, soulRelic, bodyRelic;
    
    private void Awake()
    {
        Instance = this;
    }

    public void EnableRelic(GameObject relic)
    {
        relic.SetActive(true);
    }
    
    
}
