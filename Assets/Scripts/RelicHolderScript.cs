using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicHolderScript : MonoBehaviour
{
    public static RelicHolderScript Instance;

    public GameObject mindRelic, soulRelic, bodyRelic;
    
    private bool _allRelicsFound;
    
    private void Awake()
    {
        Instance = this;
    }

    public void EnableRelic(GameObject relic)
    {
        relic.SetActive(true);
    }
    
    public bool CheckForAllRelics()
    {
        _allRelicsFound = true;

        if(!mindRelic.activeSelf) _allRelicsFound = false;
        if(!soulRelic.activeSelf) _allRelicsFound = false;
        if(!bodyRelic.activeSelf) _allRelicsFound = false;

        return _allRelicsFound;
    }
    
    
}
