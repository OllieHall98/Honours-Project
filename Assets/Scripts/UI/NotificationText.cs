using System;
using System.Collections;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace UI
{
    public enum TransitionType
    {
        Fade,
        Float
    }
    
    public class NotificationText : MonoBehaviour
    {
        private static NotificationText instance;
        public static NotificationText Instance { get { return instance; } }

        public GameObject text;

        private RectTransform _textTransform;
        private TextMeshProUGUI _textTMP;

        private Vector2 _textStartingPosition;

        [BoxGroup("Tween Parameters")] public float speed = 1f;
        [BoxGroup("Tween Parameters")] public Vector2 offset = new Vector2(0, 25f);

        private Coroutine _currentCoroutine = null;

        private void Awake()
        {
            instance = this;
            
            _textTransform =    text.GetComponent<RectTransform>();
            _textStartingPosition = _textTransform.anchoredPosition;
            _textTMP =         text.GetComponent<TextMeshProUGUI>();
            _textTMP.faceColor = Color.clear;
        }

        public void DisplayMessage(TransitionType type, string message, float duration)
        {
            if(_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _textTransform.anchoredPosition = _textStartingPosition;

            _textTMP.text = message;

            _currentCoroutine = StartCoroutine(DisplayText(type, duration));
        }

        private IEnumerator DisplayText(TransitionType type, float duration)
        {
            LTDescr tweenPosition;

            if (type == TransitionType.Float)
            {
                // Tween the text position
                var anchoredPosition = _textTransform.anchoredPosition;
                tweenPosition = LeanTween.value(text, anchoredPosition,
                    anchoredPosition + offset, speed);
                tweenPosition.setOnUpdate((Vector2 val) => { _textTransform.anchoredPosition = val; });
            }

            // Tween the text opacity
            var tweenOpacity = LeanTween.value(gameObject, 0f, 1f, speed);
            tweenOpacity.setOnUpdate((float opacity) => { _textTMP.faceColor = new Color(1, 1, 1, opacity); });

            yield return new WaitForSecondsRealtime(duration);

            if (duration == 0) yield break;

            if (type == TransitionType.Float)
            {
                // Tween the text position
                var anchoredPosition = _textTransform.anchoredPosition;
                tweenPosition = LeanTween.value(text, anchoredPosition,
                    anchoredPosition - offset, speed);
                tweenPosition.setOnUpdate((Vector2 val) => { _textTransform.anchoredPosition = val; });
            }

            // Tween the text opacity
            tweenOpacity = LeanTween.value(gameObject, 1f, 0f, speed);
            tweenOpacity.setOnUpdate((float opacity) => {  _textTMP.faceColor = new Color(1,1,1 ,opacity); });
        }
        
        public void StopDisplayingText(TransitionType type)
        {
            if (type == TransitionType.Float)
            {
                // Tween the text position
                var anchoredPosition = _textTransform.anchoredPosition;
                var tweenPosition = LeanTween.value(text, anchoredPosition,
                    anchoredPosition - offset, speed);
                tweenPosition.setOnUpdate((Vector2 val) => { _textTransform.anchoredPosition = val; });
            }

            // Tween the text opacity
            var tweenOpacity = LeanTween.value(gameObject, 1f, 0f, speed);
            tweenOpacity.setOnUpdate((float opacity) => {  _textTMP.faceColor = new Color(1,1,1 ,opacity); });
        }
        
    }
    
   
}
