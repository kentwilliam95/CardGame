using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace Pusoy
{
    public class Agent : MonoBehaviour, IGiveCard
    {
        protected List<CardView> _listCards;
        protected List<CardView> _ChoosenCards;
        protected bool isPass;
        protected int _index;        
        
        [SerializeField] private RectTransform _cardContainer;
        [SerializeField] private Image _imageBorder;

        public bool IsPass => isPass;
        public int Index => _index;
        public Func<Agent, List<CardView>, bool> OnPlayCards;
        public Action OnPass;
        public Action<int> OnCardEmpty;

        protected virtual void Start()
        {
            _ChoosenCards = new List<CardView>();
        }

        public virtual void GiveCard(CardView card)
        {
            if (_listCards == null)
                _listCards = new List<CardView>();

            _listCards.Add(card);
            card.transform.SetParent(_cardContainer);
            card.transform.localScale = Vector3.one; 
        }

        public virtual void SetTurn(bool isActive)
        {
            if (isActive)
                _imageBorder.color = Color.green;
            else
                _imageBorder.color = Color.white;
        }

        public virtual void OnReady(int index)
        {
            Global.SortPlayCards(ref _listCards);
            _index = index;
        }

        public virtual void PlayCard()
        {
            for (int i = 0; i < _ChoosenCards.Count; i++)
                _listCards.Remove(_ChoosenCards[i]);

            OnPlayCards?.Invoke(this, _ChoosenCards);
            _ChoosenCards.Clear();
            if (_listCards.Count == 0)
            {
                OnCardEmpty?.Invoke(_index);
            }
        }

        public virtual void Pass()
        {
            if (!isPass)
            {
                isPass = true;
                OnPass?.Invoke();
            }
        }

        public void ResetState()
        {
            isPass = false;
        }

        public virtual void OnUpdate(GameController.CardPlayed cardPlayed, int totalCard, int tablePoint)
        {
            
        }

        public bool HaveThreeOfDiamond()
        {
            if (_listCards == null)
                return false;

            if (_listCards.Count == 0)
                return false;

            bool result = false;
            for (int i = 0; i < _listCards.Count; i++)
            {
                if (_listCards[i].Card.value == 3 && _listCards[i].Card.cardType == DatabaseSO.CardType.Diamond)
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
