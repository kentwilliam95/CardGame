using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pusoy
{
    [CreateAssetMenu(menuName = "Asset/Database", fileName = "Database")]
    public class DatabaseSO : ScriptableObject
    {
        public enum CardType
        {
            Diamond = 0,
            Club = 2,
            Heart = 4,
            Spades = 8
        }

        [System.Serializable]
        public class CardData
        {
            public Sprite sprite;
            public CardType cardType;
            public int value;
            public int cardDatabseIndex;
            public int totalValue;
            public string cardName;
        }

        [System.Serializable]
        public struct CardDataHeader
        {
            public CardType cardType;
            public Sprite[] sprites;
        }

        private Dictionary<int, Sprite> _cardDictionary;

        public CardDataHeader[] data;

        public void Initialize()
        {
            _cardDictionary = new Dictionary<int, Sprite>();
            for (int i = 0; i < data.Length; i++)
            {
                int number = 2;
                var sprites = data[i].sprites;
                for (int j = 0; j < sprites.Length; j++)
                {
                    _cardDictionary.Add((int)data[i].cardType * 1000 + number, sprites[j]);
                    number += 1;
                }
            }
        }

        public Sprite GetSprite(int id)
        {
            return _cardDictionary[id];
        }
    }
}
