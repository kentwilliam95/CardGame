using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace Pusoy
{
    public class ResultController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private CanvasGroup _canvasGroup;
        public void Show(string text)
        {
            _winnerText.text = $"Winner: {text}";
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
