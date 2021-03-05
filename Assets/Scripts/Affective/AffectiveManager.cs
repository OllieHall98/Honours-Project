using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Affective
{
    public class AffectiveManager : MonoBehaviour
    {
        private EmotionType _neutral;
        private EmotionType _joy;
        private EmotionType _sadness;
        private EmotionType _fear;
        private EmotionType _disgust;
        private EmotionType _anger;
        private EmotionType _surprise;

        private UnityEvent _neutralEvent;
        private UnityEvent _joyEvent;
        private UnityEvent _sadnessEvent;
        private UnityEvent _fearEvent;
        private UnityEvent _disgustEvent;
        private UnityEvent _angerEvent;
        private UnityEvent _surpriseEvent;

        [SerializeField] [Range(0,10)] private float joyValue;
        [SerializeField] [Range(0,10)] private float sadnessValue;
        [SerializeField] [Range(0,10)] private float fearValue;
        [SerializeField] [Range(0,10)] private float disgustValue;
        [SerializeField] [Range(0,10)] private float angerValue;
        [SerializeField] [Range(0,10)] private float surpriseValue;

        Queue<string> _lastEmotions;

        [SerializeField] string currentEmotion;

        [SerializeField] float affectiveValue;

        public AffectiveManager(EmotionType neutral)
        {
            _neutral = neutral;
        }

        public class EmotionType
        {
            public string Label;
            public float Intensity;

            public EmotionType(string label, float intensity)
            {
                Label = label;
                Intensity = intensity;
            }
        }

        private void Awake()
        {
            InitialiseEvents();
            InitialiseEmotions();

            _lastEmotions = new Queue<string>();
        }

        void InitialiseEvents()
        {
            _neutralEvent    = new UnityEvent();
            _joyEvent        = new UnityEvent();
            _sadnessEvent    = new UnityEvent();
            _fearEvent       = new UnityEvent();
            _disgustEvent    = new UnityEvent();
            _angerEvent      = new UnityEvent();
            _surpriseEvent   = new UnityEvent();

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Affective"))
            {
                _neutralEvent.AddListener(obj.GetComponent<ObjectState>().a_Neutral);
                _joyEvent.AddListener(obj.GetComponent<ObjectState>().a_Joy);
                _sadnessEvent.AddListener(obj.GetComponent<ObjectState>().a_Sadness);
                _fearEvent.AddListener(obj.GetComponent<ObjectState>().a_Fear);
                _disgustEvent.AddListener(obj.GetComponent<ObjectState>().a_Disgust);
                _angerEvent.AddListener(obj.GetComponent<ObjectState>().a_Anger);
                _surpriseEvent.AddListener(obj.GetComponent<ObjectState>().a_Surprise);
            }
        }

        private void InitialiseEmotions()
        {
            _joy = new EmotionType("Joy", joyValue);
            _sadness = new EmotionType("Sadness", sadnessValue);
            _fear = new EmotionType("Fear", fearValue);
            _disgust = new EmotionType("Disgust", disgustValue);
            _anger = new EmotionType("Anger", angerValue);
            _surprise = new EmotionType("Surprise", surpriseValue);
        }

        private void UpdateAffectiveValues()
        {
            _joy.Intensity       = joyValue;
            _sadness.Intensity   = sadnessValue;
            _fear.Intensity      = fearValue;
            _disgust.Intensity   = disgustValue;
            _anger.Intensity     = angerValue;
            _surprise.Intensity  = surpriseValue;

            affectiveValue += _joy.Intensity;
            affectiveValue -= _sadness.Intensity;
            affectiveValue -= _fear.Intensity;
            affectiveValue -= _disgust.Intensity;
            affectiveValue -= _anger.Intensity;
            affectiveValue += _surprise.Intensity;
        }

        private void RaiseEvent(UnityEvent e)
        {
            if (e == null)
            {
                Debug.LogError("Could not find event!");
                return;
            }

            //CancelInvoke();
            //e.Invoke();

        }

        public void SetCurrentEmotion(string emotion)
        {
            if (emotion == "N/A")
            { 
                Debug.Log("No Face Detected");
                return;
            }
                

            string strongestEmotion = CalculateStrongestEmotion(emotion);

            if (strongestEmotion == null || strongestEmotion == currentEmotion)
                return;

            Debug.Log("Strongest emotion is now " + strongestEmotion);

            currentEmotion = strongestEmotion;

            switch (strongestEmotion)
            {
                case "neutral":
                    RaiseEvent(_neutralEvent);
                    break;
                case "surprise":
                    RaiseEvent(_surpriseEvent);
                    break;
                case "sadness":
                    RaiseEvent(_sadnessEvent);
                    break;
                case "fear":
                    RaiseEvent(_fearEvent);
                    break;
                case "disgust":
                    RaiseEvent(_disgustEvent);
                    break;
                case "anger":
                    RaiseEvent(_angerEvent);
                    break;
                case "joy":
                    RaiseEvent(_joyEvent);
                    break;

            }
        }

        private string CalculateStrongestEmotion(string emotion)
        {
            if (_lastEmotions.Count >= 10) _lastEmotions.Dequeue();
            _lastEmotions.Enqueue(emotion);

            string[] emotions = _lastEmotions.ToArray();

            var groups = emotions.GroupBy(s => s);

            int highestGroupCount = 0;
            string strongestEmotion = null;

            foreach (var group in groups)
            {
                if (group.Count() > highestGroupCount)
                {
                    highestGroupCount = group.Count();
                    strongestEmotion = group.Key;
                }
            }

            return strongestEmotion;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                RaiseEvent(_neutralEvent);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) RaiseEvent(_joyEvent);
            if (Input.GetKeyDown(KeyCode.Alpha3)) RaiseEvent(_sadnessEvent);
            if (Input.GetKeyDown(KeyCode.Alpha4)) RaiseEvent(_fearEvent);
            if (Input.GetKeyDown(KeyCode.Alpha5)) RaiseEvent(_disgustEvent);
            if (Input.GetKeyDown(KeyCode.Alpha6)) RaiseEvent(_angerEvent);
            if (Input.GetKeyDown(KeyCode.Alpha7)) RaiseEvent(_surpriseEvent);
        
        

            UpdateAffectiveValues();

            if (affectiveValue > 0.5f)
            {
                // Do happy stuff
            }
            else
            {
                // Do sad stuff
            }
        }
    }
}
