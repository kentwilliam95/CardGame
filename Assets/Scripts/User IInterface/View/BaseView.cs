using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pusoy
{
    public class BaseView : MonoBehaviour
    {
        private Coroutine _coroutine;
        [SerializeField] private CanvasGroup _canvasGroup;
        public virtual void Show(Action onComplete = null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(Global.FadeIenumerator(_canvasGroup.alpha, 1, 0.2f, CanvasGroupAlpha_OnUpdate, onComplete));
        }

        public virtual void Hide(Action onComplete = null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(Global.FadeIenumerator(_canvasGroup.alpha, 0, 0.2f, CanvasGroupAlpha_OnUpdate, onComplete));
        }

        private void CanvasGroupAlpha_OnUpdate(float value)
        {
            _canvasGroup.alpha = value;
        }
    }
}
