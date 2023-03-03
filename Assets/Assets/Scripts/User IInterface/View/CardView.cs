using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Pusoy
{
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        private DatabaseSO.CardData _card;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _text;
        
        public DatabaseSO.CardData Card => _card;
        public System.Action<CardView> OnClick;
        public void Initialize(DatabaseSO.CardData card, DatabaseSO database)
        {            
            _card = card;
            _image.sprite = database.GetSprite(_card.cardDatabseIndex);
            _text.text = card.cardName;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
        }

        public void Click()
        {
            _image.color = Color.yellow;
        }

        public void UnClick()
        {
            _image.color = Color.white;
        }
    }
}
