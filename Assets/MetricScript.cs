using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricScript : MonoBehaviour
{
    public static MetricScript Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void LogMetric(string s)
    {
        Debug.Log(s);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
