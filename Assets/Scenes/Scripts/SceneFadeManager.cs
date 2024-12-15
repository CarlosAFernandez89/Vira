using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Scripts
{
    public class SceneFadeManager : MonoBehaviour
    {
        public static SceneFadeManager Instance;
        
        [SerializeField] private Image fadeImage;
        [Range(0.1f,10f),SerializeField] private float fadeInSpeed = 0.5f;
        [Range(0.1f,10f),SerializeField] private float fadeOutSpeed = 0.5f;

        [Range(0.1f,1f),SerializeField] private float fadeInWaitTime = 0.25f;

        [SerializeField] private Color fadeOutStartColor;

        private float _waitTime = 0f;
        
        public bool IsFadingOut { get; private set; }
        public bool IsFadingIn { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            fadeOutStartColor.a = 0f;
        }

        private void Update()
        {
            if (IsFadingOut)
            {
                if (fadeImage.color.a < 1f)
                {
                    fadeOutStartColor.a += fadeOutSpeed * Time.deltaTime;
                    fadeImage.color = fadeOutStartColor;
                }
                else
                {
                    IsFadingOut = false;
                    _waitTime = fadeInWaitTime;
                    
                    Debug.Log("Fade Out Complete");

                }
            }

            if (IsFadingIn)
            {
                _waitTime -= Time.deltaTime;
                if (fadeImage.color.a > 0f && _waitTime <= 0)
                {
                    fadeOutStartColor.a -= fadeInSpeed * Time.deltaTime;
                    fadeImage.color = fadeOutStartColor;
                }
                else if(_waitTime <= 0)
                {
                    IsFadingIn = false;
                    Debug.Log("Fade In Complete");
                }
            }
        }

        public void FadeOut()
        {
            fadeImage.color = fadeOutStartColor;
            IsFadingOut = true;
        }

        public void FadeIn()
        {
            if (fadeImage.color.a >= 1f)
            {
                fadeImage.color = fadeOutStartColor;
                IsFadingIn = true;
            }
        }
    }
}
