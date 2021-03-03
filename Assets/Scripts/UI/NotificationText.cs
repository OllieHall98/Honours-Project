using System.Collections;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace UI
{
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

        void Start()
        {
            if(instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }

            if(text == null)
            {
                Debug.LogError("Notification text not assigned");
                return;
            }

            _textTransform =    text.GetComponent<RectTransform>();
            _textTMP =         text.GetComponent<TextMeshProUGUI>();
            _textTMP.faceColor = Color.clear;

            _textStartingPosition = _textTransform.anchoredPosition;

        }

        public void DisplayMessage(string message, float duration)
        {
            if(_currentCoroutine != null)
            {
                StopCoroutine(_currentCoroutine);
            }

            _textTransform.anchoredPosition = _textStartingPosition;

            _textTMP.text = message;

            _currentCoroutine = StartCoroutine(DisplayText(duration));
        }

        private IEnumerator DisplayText(float duration)
        {
            // Tween the text position
            var tweenPosition = LeanTween.value(text, _textTransform.anchoredPosition, _textTransform.anchoredPosition + offset, speed);
            tweenPosition.setOnUpdate((Vector2 val) => { _textTransform.anchoredPosition = val; });

            // Tween the text opacity
            var tweenOpacity = LeanTween.value(gameObject, Color.clear, Color.white, speed);
            tweenOpacity.setOnUpdate((Color opacity) => { _textTMP.faceColor = opacity; });

            yield return new WaitForSecondsRealtime(duration);

            // Tween the text position
            tweenPosition = LeanTween.value(text, _textTransform.anchoredPosition, _textTransform.anchoredPosition - offset, speed);
            tweenPosition.setOnUpdate((Vector2 val) => { _textTransform.anchoredPosition = val; });

            // Tween the text opacity
            tweenOpacity = LeanTween.value(gameObject, Color.white, Color.clear, speed);
            tweenOpacity.setOnUpdate((Color opacity) => { _textTMP.faceColor = opacity; });
        }
    }
}
