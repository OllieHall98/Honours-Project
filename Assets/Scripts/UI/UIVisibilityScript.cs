using System;
using UnityEngine;

    public class UIVisibilityScript : MonoBehaviour
    {
        public static UIVisibilityScript Instance;

        private CanvasGroup _uiGroup;
        
        public float speed = 1f;


        private void Awake()
        {
            _uiGroup = GetComponent<CanvasGroup>();
            Instance = this;
        }

        public void ShowUI()
        {
            var tweenOpacity = LeanTween.value(gameObject, 0,1, speed);
            tweenOpacity.setOnUpdate((float opacity) => { _uiGroup.alpha = opacity; });
        }

        public void HideUI()
        {
            var tweenOpacity = LeanTween.value(gameObject, 1,0, speed);
            tweenOpacity.setOnUpdate((float opacity) => { _uiGroup.alpha = opacity; });
        }
    }