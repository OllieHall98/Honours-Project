using System;
using UnityEngine;

    public class UIVisibilityScript : MonoBehaviour
    {
        public static UIVisibilityScript Instance;

        private CanvasGroup _uiGroup;


        private void Awake()
        {
            _uiGroup = GetComponent<CanvasGroup>();
            Instance = this;
        }

        public void ShowUI(float speed)
        {
            var tweenOpacity = LeanTween.value(gameObject, 0,1, speed);
            tweenOpacity.setOnUpdate((float opacity) => { _uiGroup.alpha = opacity; });
        }

        public void HideUI(float speed)
        {
            var tweenOpacity = LeanTween.value(gameObject, 1,0, speed);
            tweenOpacity.setOnUpdate((float opacity) => { _uiGroup.alpha = opacity; });
        }
    }