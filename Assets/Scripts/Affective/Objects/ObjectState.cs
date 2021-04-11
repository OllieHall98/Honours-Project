using UnityEngine;
using UnityEngine.Events;

namespace Affective
{
    public class ObjectState : MonoBehaviour
    {
        public enum Label { Joy, Sadness, Fear, Disgust, Anger, Surprise };
        Label _label;
        float _intensity;

        public UnityAction ANeutral;
        public UnityAction AJoy;
        public UnityAction ASadness;
        public UnityAction AFear;
        public UnityAction ADisgust;
        public UnityAction AAnger;
        public UnityAction ASurprise;

        public void Awake()
        {
            ANeutral   +=   Neutral_State;
            AJoy       +=   Joy_State;
            ASadness   +=   Sadness_State;
            AFear      +=   Fear_State;
            ADisgust   +=   Disgust_State;
            AAnger     +=   Anger_State;
            ASurprise  +=   Surprise_State;
        }

        public void SetObjectState(Label label, float intensity)
        {
            _label = label;
            _intensity = intensity;
        }

        public virtual void Neutral_State() { Debug.Log(this.gameObject.name + " is neutral"); }
        public virtual void Joy_State() { Debug.Log(this.gameObject.name + " is joyful"); }
        public virtual void Sadness_State() { Debug.Log(this.gameObject.name + " is sad"); }
        public virtual void Fear_State() { Debug.Log(this.gameObject.name + " is fearful"); }
        public virtual void Disgust_State() { Debug.Log(this.gameObject.name + " is disgusted"); }
        public virtual void Anger_State() { Debug.Log(this.gameObject.name + " is angry"); }
        public virtual void Surprise_State() { Debug.Log(this.gameObject.name + " is surprised"); }
    }
}
