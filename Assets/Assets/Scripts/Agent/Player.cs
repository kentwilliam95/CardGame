using UnityEngine;
using System.Collections.Generic;

namespace Pusoy
{
    public class Player : Agent
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        public override void OnUpdate(GameController.CardPlayed cardPlayed, int totalCard, int tablePoint)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                base.PlayCard();
            }
        }

        public override void OnReady(int index)
        {
            this._index = index;
        }
        
        public override void GiveCard(CardView card)
        {
            base.GiveCard(card);
            card.OnClick = Card_OnClick;
        }

        public override void SetTurn(bool isActive)
        {
            base.SetTurn(isActive);
            _canvasGroup.interactable = isActive;
            _canvasGroup.blocksRaycasts = isActive;
        }

        public override void PlayCard()
        {
            base.PlayCard();
        }
        
        private void Card_OnClick(CardView cardView)
        {
            if (_ChoosenCards.Count >= 5)
                return;

            if(!_ChoosenCards.Contains(cardView))
                _ChoosenCards.Add(cardView);

            cardView.Click();
            Debug.Log(cardView.Card.cardName);
        }
    }
}
