using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MetricScript : MonoBehaviour
{
    private void Awake()
    {
        LogMetric("NEW GAME");
    }

    public static void LogMetric(string s)
    {
        var timeSpan = TimeSpan.FromSeconds((Time.time));
        var timeText = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        
        string text = "(" + timeText + ") " + s;
        
        Debug.Log(text);
        
        // Write to file
        var writer = new StreamWriter(Application.dataPath + "/StreamingAssets/metrics.txt", true);
        writer.WriteLine(text);
        writer.Close();
    }
}
