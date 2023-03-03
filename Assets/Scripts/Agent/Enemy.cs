using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pusoy
{
    public class Enemy : Agent
    {
        private Dictionary<int, List<CardView>> tableComboFullHouseAndForOfKinds;
        private Dictionary<int, List<CardView>> tableComboFlush;
        private Dictionary<int, List<CardView>> tableComboPair;
        private List<int> dictIndex = new List<int>();
        private float delay = 0.75f;

        protected override void Start()
        {
            base.Start();
            tableComboFullHouseAndForOfKinds = new Dictionary<int, List<CardView>>();
            tableComboFlush = new Dictionary<int, List<CardView>>();
            tableComboPair = new Dictionary<int, List<CardView>>();
        }

        public override void OnUpdate(GameController.CardPlayed cardPlayed, int totalCard, int tablePoint)
        {
            if (isPass || GameController._state != GameController.State.Play)
                return;

            delay -= Time.deltaTime;
            if (delay > 0)
                return;

            _ChoosenCards.Clear();
            ArrangeCombo();

            switch (cardPlayed)
            {
                case GameController.CardPlayed.None:
                    var value = CheckCanPlayCard(tablePoint);
                    if (value)
                        PlayCard();
                    break;

                case GameController.CardPlayed.Single:
                    if (PlayOneCard(tablePoint))
                        PlayCard();
                    else
                        Pass();
                    break;

                case GameController.CardPlayed.Pair:
                    var canPlayPair = PlayPairCard(tablePoint);
                    if (canPlayPair)
                        PlayCard();
                    else
                        Pass();
                    break;
            }
        }

        public override void PlayCard()
        {
            base.PlayCard();
            delay = 1f;
        }

        public override void Pass()
        {
            base.Pass();
            delay = 1f;
        }

        private bool CheckCanPlayCard(int tablePoint)
        {
            bool canPlayCard = false;
            bool canPlayPair = false;
            bool canPlayOneCard = false;
            bool canPlayTripleCard = false;

            if (PlayTripleCard(tablePoint))
            {
                canPlayTripleCard = true;
                Debug.Log("Play Triple");
            }

            if (!canPlayTripleCard && PlayPairCard(tablePoint))
            {
                canPlayPair = true;
                Debug.Log("Play Pair");
            }

            if (!canPlayPair && !canPlayTripleCard)
            {
                canPlayOneCard = PlayOneCard(tablePoint);
            }

            canPlayCard = canPlayPair || canPlayOneCard || canPlayTripleCard;
            return canPlayCard;
        }

        private bool PlayFourOfKinds(GameController.CardPlayed cardPlayed, int tablePoint)
        {
            dictIndex.Clear();
            bool canPlayCards = false;
            bool firstTurn = HaveThreeOfDiamond();

            int otherNumber = tablePoint % 1000;
            int otherSuit = tablePoint / 1000;
            
            if (firstTurn)
            {
                var list = tableComboFullHouseAndForOfKinds[3];
                if (list.Count >= 4)
                {

                }
                else
                {
                    foreach (var item in tableComboFullHouseAndForOfKinds)
                    {
                        if (item.Value.Count >= 4)
                        {
                            dictIndex.Add(item.Key);
                            break;
                        }
                    }
                }

            }
            else
            {
                foreach (var kvp in tableComboFullHouseAndForOfKinds)
                {
                    var value = kvp.Value;
                    if (value.Count >= 4)
                    {
                        if (value[0].Card.value * 4 > tablePoint)
                        {
                            for (int i = value.Count - 1; i >= 0; i--)
                            {
                                _ChoosenCards.Add(value[i]);
                                _listCards.Remove(value[i]);
                            }

                            bool isHaveThreeDiamonds = HaveThreeOfDiamond();
                            if (isHaveThreeDiamonds)
                            {
                                for (int i = 0; i < _listCards.Count; i++)
                                {
                                    var cardView = _listCards[i];
                                    if (_listCards[i].Card.value == 3 && _listCards[i].Card.cardType == DatabaseSO.CardType.Diamond)
                                    {
                                        _ChoosenCards.Add(cardView);
                                        _listCards.Remove(cardView);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var cardView = _listCards[UnityEngine.Random.Range(0, _listCards.Count)];
                                _ChoosenCards.Add(cardView);
                                _listCards.Remove(cardView);
                            }
                            canPlayCards = true;
                            break;
                        }
                    }
                }
            }

            return canPlayCards;
        }
        private bool PlayFullHouse(GameController.CardPlayed cardPlayed, int tablePoint)
        {
            if (GameController.CardPlayed.FullHouse < cardPlayed)
                return false;

            return true;
        }

        private bool PlayTripleCard(int tablePoint)
        {
            bool canPlayCard = false;
            dictIndex.Clear();
            bool firstTurn = HaveThreeOfDiamond();
            if (firstTurn)
            {
                bool haveThree = tableComboFullHouseAndForOfKinds.ContainsKey(3);
                var list = tableComboFullHouseAndForOfKinds[3];
                if (list.Count >= 3)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        _ChoosenCards.Add(item);
                        _listCards.Remove(item);
                    }
                    canPlayCard = true;
                }
            }
            else
            {
                foreach (var kvp in tableComboFullHouseAndForOfKinds)
                {
                    var list = kvp.Value;
                    if (list.Count >= 3)
                        dictIndex.Add(kvp.Key);
                }
            }

            if (dictIndex.Count > 0)
            {
                var key = dictIndex[UnityEngine.Random.Range(0, dictIndex.Count)];
                var listCardView = tableComboFullHouseAndForOfKinds[key];
                for (int i = 0; i < listCardView.Count; i++)
                {
                    var item = listCardView[i];
                    _ChoosenCards.Add(item);
                    _listCards.Remove(item);
                }
                canPlayCard = true;
            }

            return canPlayCard;
        }

        private bool PlayPairCard(int tablePoint)
        {
            bool canPlaycard = false;
            tableComboPair.Clear();
            bool firstTurn = HaveThreeOfDiamond();

            if (firstTurn)
            {
                var items = tableComboFullHouseAndForOfKinds[3];
                if (items.Count < 2)
                    return canPlaycard;
                else
                {
                    int threeDiamondIndex = 0;
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].Card.value == 3 && items[i].Card.cardType == DatabaseSO.CardType.Diamond)
                        {
                            threeDiamondIndex = i;
                            break;
                        }
                    }

                    _ChoosenCards.Add(items[threeDiamondIndex]);
                    items.Remove(items[threeDiamondIndex]);

                    _ChoosenCards.Add(items[0]);
                    items.Remove(items[0]);

                    canPlaycard = true;
                }
            }
            else
            {
                foreach (var kvp in tableComboFullHouseAndForOfKinds)
                {
                    if (kvp.Value.Count >= 2)
                    {
                        int otherNumber = tablePoint % 1000;
                        int otherSuit = tablePoint / 1000;
                        bool addToTable = false;
                        int breakIndex = 0;
                        int lowest = 0;
                        for (int i = 0; i < kvp.Value.Count; i++)
                        {
                            int currentNumber = kvp.Value[i].Card.value;
                            int currentSuit = (int)kvp.Value[i].Card.cardType;
                            bool checkifReplaceCard = otherNumber < currentNumber || (otherNumber == currentNumber && otherSuit < currentSuit);

                            if (checkifReplaceCard)
                            {
                                addToTable = true;
                                breakIndex = i;
                                break;
                            }
                            else
                            {
                                lowest = i;
                            }
                        }

                        if (addToTable)
                        {
                            CardView item1 = null;
                            CardView item2 = null;
                            if (breakIndex - 1 < 0)
                            {
                                item1 = kvp.Value[breakIndex];
                                item2 = kvp.Value[breakIndex + 1];
                            }
                            else if (breakIndex + 1 > 2)
                            {
                                item1 = kvp.Value[breakIndex];
                                item2 = kvp.Value[breakIndex - 1];
                            }

                            _ChoosenCards.Add(item1);
                            _ChoosenCards.Add(item2);
                            _listCards.Remove(item1);
                            _listCards.Remove(item2);

                            canPlaycard = true;
                            break;
                        }
                    }
                }
            }
            return canPlaycard;
        }

        private bool PlayOneCard(int tablePoint)
        {
            bool canPlayOneCard = false;

            int otherNumber = tablePoint % 1000;
            int otherSuit = tablePoint / 1000;
            List<CardView> potentialCard = new List<CardView>();

            for (int i = 0; i < _listCards.Count; i++)
            {
                int currentNumber = _listCards[i].Card.value == 2 ? 17 : _listCards[i].Card.value;
                int currentSuit = (int)_listCards[i].Card.cardType;
                bool haveThreeDiamond = _listCards[i].Card.value == 3 && _listCards[i].Card.cardType == DatabaseSO.CardType.Diamond;
                bool checkifReplaceCard = otherNumber < currentNumber || (otherNumber == currentNumber && otherSuit < currentSuit);

                if (haveThreeDiamond)
                {
                    potentialCard.Clear();
                    potentialCard.Add(_listCards[i]);
                    break;
                }
                else if (checkifReplaceCard)
                {
                    potentialCard.Add(_listCards[i]);
                }
            }

            if (potentialCard.Count > 0)
            {
                int i = UnityEngine.Random.Range(0, potentialCard.Count);
                _ChoosenCards.Add(potentialCard[i]);
                _listCards.Remove(potentialCard[i]);
                canPlayOneCard = true;
            }

            return canPlayOneCard;
        }

        private void ArrangeCombo()
        {
            tableComboFlush.Clear();
            tableComboFullHouseAndForOfKinds.Clear();

            for (int i = 0; i < _listCards.Count; i++)
            {
                if (!tableComboFullHouseAndForOfKinds.ContainsKey(_listCards[i].Card.value))
                    tableComboFullHouseAndForOfKinds.Add(_listCards[i].Card.value, new List<CardView>() { _listCards[i] });
                else
                    tableComboFullHouseAndForOfKinds[_listCards[i].Card.value].Add(_listCards[i]);
            }
        }

    }
}
