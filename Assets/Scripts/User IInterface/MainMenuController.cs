using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace Pusoy
{
    public class MainMenuController : MonoBehaviour
    {
        public enum ViewType
        {
            Help,
            Menu,
            Win,
            Lose
        }

        private Coroutine coroutinePanelFade;
        private BaseView _currentView;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private BaseView[] _views;

        public void Show(Action onComplete = null)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            ShowMenuView();

            if (coroutinePanelFade != null)
                StopCoroutine(coroutinePanelFade);

            coroutinePanelFade = StartCoroutine(Global.FadeIenumerator(_canvasGroup.alpha, 1, 0.2f, CanvasGroupAlpha_OnUpdate, onComplete));
        }

        public void ShowHelpView()
        {
            _views[(int)ViewType.Menu].Hide(() =>
            {
                _views[(int)ViewType.Help].Show();
            });

        }

        public void ShowMenuView()
        {
            _views[(int)ViewType.Help].Hide(() =>
            {
                _views[(int)ViewType.Menu].Show();
            });
        }

        public void Hide(Action onComplete = null)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            
            if (coroutinePanelFade != null)
                StopCoroutine(coroutinePanelFade);

            coroutinePanelFade = StartCoroutine(Global.FadeIenumerator(_canvasGroup.alpha, 0, 0.25f, CanvasGroupAlpha_OnUpdate, onComplete));
        }

        private void CanvasGroupAlpha_OnUpdate(float value)
        {
            _canvasGroup.alpha = value;
        }
    }
}
