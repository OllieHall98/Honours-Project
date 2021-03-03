using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectState : MonoBehaviour
{
    public enum Label { Joy, Sadness, Fear, Disgust, Anger, Surprise };
    Label label;

    float intensity;

    public UnityAction a_Neutral;
    public UnityAction a_Joy;
    public UnityAction a_Sadness;
    public UnityAction a_Fear;
    public UnityAction a_Disgust;
    public UnityAction a_Anger;
    public UnityAction a_Surprise;

    private void Awake()
    {
        a_Neutral   +=   Neutral_State;
        a_Joy       +=   Joy_State;
        a_Sadness   +=   Sadness_State;
        a_Fear      +=   Fear_State;
        a_Disgust   +=   Disgust_State;
        a_Anger     +=   Anger_State;
        a_Surprise  +=   Surprise_State;
    }

    public void SetObjectState(Label label_, float intensity_)
    {
        label = label_;
        intensity = intensity_;
    }

    public virtual void Neutral_State() { Debug.Log(this.gameObject.name + " is neutral"); }
    public virtual void Joy_State() { Debug.Log(this.gameObject.name + " is joyful"); }
    public virtual void Sadness_State() { Debug.Log(this.gameObject.name + " is sad"); }
    public virtual void Fear_State() { Debug.Log(this.gameObject.name + " is fearful"); }
    public virtual void Disgust_State() { Debug.Log(this.gameObject.name + " is disgusted"); }
    public virtual void Anger_State() { Debug.Log(this.gameObject.name + " is angry"); }
    public virtual void Surprise_State() { Debug.Log(this.gameObject.name + " is surprised"); }
}
