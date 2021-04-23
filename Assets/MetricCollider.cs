using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "Player") return;
        
        string s = gameObject.name + " entered, emotion is " + AffectiveManager.Instance.GetCurrentEmotion();
        MetricScript.Instance.LogMetric(s);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name != "Player") return;
        
        string s = gameObject.name + " exited, emotion is " + AffectiveManager.Instance.GetCurrentEmotion();
        MetricScript.Instance.LogMetric(s);
    }
}
