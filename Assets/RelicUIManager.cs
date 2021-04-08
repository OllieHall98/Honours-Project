using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class RelicUIManager : MonoBehaviour
{
    public static RelicUIManager Instance;
    
    public Image mindRelic, bodyRelic, soulRelic;

    private void Awake()
    {
        Instance = this;

        mindRelic.enabled = false;
        bodyRelic.enabled = false;
        soulRelic.enabled = false;
    }

    public void EnableRelicUI(Image relic)
    {
        relic.enabled = true;
    }
    
    
}
