using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Pusoy
{
    public class GameController : MonoBehaviour
    {
        public enum CardPlayed
        {
            None = 0,
            Single = 1,
            Pair = 2,
            Triple = 3,
            Straight = 5,
            Flush = 6,
            FullHouse = 7,
            FourOfKind = 8
        }

        public enum State
        {
            MainMenu,
            GiveCard,
            Play,
            End
        }

        public static State _state;
        private int _restartCount;
        private Coroutine _spawnCardsCoroutine;
        private Dictionary<int, DatabaseSO.CardData> _cardDictionary;
        private List<DatabaseSO.CardData> _cardList;
        private CardPlayed _currentCardPlayed = CardPlayed.None;
        private CardPlayed _evaluationCardPlayed = CardPlayed.None;
        private int _currentPlayPoint;
        private Agent _choosenAgent;
        private Agent _currentAgentTurn;
        private int _currentAgentTurnIndex;
        private int _currentTotalCard;
        private int _winnerAgentIndexPerRound;        

        [SerializeField] private Agent[] agents;
        [SerializeField] private PoolSpawnId _cardViewTemplateId;
        [SerializeField] private DatabaseSO _database;
        [SerializeField] private RectTransform _middleCardDisplay;
        [SerializeField] private MainMenuController _mainMenuController;
        [SerializeField] private ResultController _resultController;

        [Header("AudioClip")]
        [SerializeField] private AudioClip _cardShuffleAudioClip;
        [SerializeField] private AudioClip _cardPlayAudioClip;
        [SerializeField] private AudioClip _cardInCorrectAudioClip;
        [SerializeField] private AudioClip _passAudioClip;
        [SerializeField] private AudioClip _gameEndAudioClip;

        private void Start()
        {
            _database.Initialize();
            _state = State.MainMenu;
            _mainMenuController.Show();
            _resultController.Hide();
        }

        public void StartGame()
        {
            _mainMenuController.Hide(() =>
            {
                GenerateCardDictionary();
                if (_spawnCardsCoroutine != null)
                    StopCoroutine(_spawnCardsCoroutine);

                _spawnCardsCoroutine = StartCoroutine(SpawnCardCoroutine(FinishGiveCards));
            });
        }

        private void Update()
        {
            if (_state == State.Play)
            {
                if (!agents[_currentAgentTurnIndex].IsPass)
                    agents[_currentAgentTurnIndex].OnUpdate(_currentCardPlayed, _currentTotalCard, _currentPlayPoint);
            }
        }

        private void FinishGiveCards()
        {
            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].OnPlayCards = Agent_OnPlayCards;
                agents[i].OnPass = Agent_OnPass;
                agents[i].OnCardEmpty = Agent_OnCardEmpty;
                bool haveThreeDiamond = agents[i].HaveThreeOfDiamond();
                agents[i].SetTurn(haveThreeDiamond);
                if (haveThreeDiamond)
                {
                    _choosenAgent = agents[i];
                    _currentAgentTurn = _choosenAgent;
                    _currentAgentTurnIndex = i;
                }

                agents[i].OnReady(i);
            }

            _state = State.Play;
        }

        private void Agent_OnPass()
        {
            AudioManager.Instance.PlaySfx(_passAudioClip);
            NextTurn();
        }

        private void NextTurn()
        {
            int passCount = 0;
            int max = agents.Length;
            for (int i = 0; i < agents.Length; i++)
                agents[i].SetTurn(false);

            for (int i = 0; i < 4; i++)
            {
                _currentAgentTurnIndex += 1;
                if (_currentAgentTurnIndex >= max)
                    _currentAgentTurnIndex = 0;

                if (agents[_currentAgentTurnIndex].IsPass)
                    passCount += 1;
                else
                {
                    agents[_currentAgentTurnIndex].SetTurn(true);
                    break;
                }
            }

            if (passCount >= 3)
            {
                Debug.Log("NextRound");
                NextRound();
            }
        }

        private void NextRound()
        {
            ClearMiddleCards();

            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].SetPass(false);
                agents[i].SetTurn(false);
            }

            _currentAgentTurnIndex = _winnerAgentIndexPerRound;
            agents[_currentAgentTurnIndex].SetTurn(true);

            _currentCardPlayed = CardPlayed.None;
            _currentPlayPoint = 0;
        }

        private void Agent_OnCardEmpty(int agentIndex)
        {
            _state = State.End;
            AudioManager.Instance.PlaySfx(_gameEndAudioClip);
            ShowResult(agents[agentIndex].name);
        }

        private bool Agent_OnPlayCards(Agent agent, List<CardView> playCards)
        {
            Global.SortPlayCards(ref playCards);
            int point = 0;
            switch (playCards.Count)
            {
                case 1:
                    _evaluationCardPlayed = CardPlayed.Single;
                    int value = playCards[0].Card.value == 2 ? 17 : playCards[0].Card.value;
                    point = (int)playCards[0].Card.cardType * 1000 + value;
                    break;
                case 2:
                    if (CheckCardsIfPair(ref playCards, out point))
                        _evaluationCardPlayed = CardPlayed.Pair;
                    break;

                case 3:
                    if (CheckCardsIfTriple(ref playCards, out point))
                        _evaluationCardPlayed = CardPlayed.Triple;
                    break;
                case 4:
                    _evaluationCardPlayed = CardPlayed.None;
                    break;
                case 5:
                    if (CheckCardsIfStraight(ref playCards, out point))
                        _evaluationCardPlayed = CardPlayed.Straight;
                    else if (CheckCardIfFlush(ref playCards, out point))
                        _evaluationCardPlayed = CardPlayed.Flush;
                    else if (CheckCardIfFullHouse(ref playCards, out point))
                        _evaluationCardPlayed = CardPlayed.FullHouse;
                    else if (CheckCardIfFourOfKinds(ref playCards, out point))
                        _evaluationCardPlayed = CardPlayed.FourOfKind;
                    else
                        _evaluationCardPlayed = CardPlayed.None;
                    break;
            }

            // Debug.Log($"Evaluation : {_evaluationCardPlayed}");

            bool isValidToCheckPoint = false;
            bool isReplaceCard = false;
            if (_currentCardPlayed == CardPlayed.None)
            {
                if (_evaluationCardPlayed != CardPlayed.None)
                {
                    _currentCardPlayed = _evaluationCardPlayed;
                    _currentPlayPoint = point;
                    isReplaceCard = true;
                    _currentTotalCard = playCards.Count;
                    _winnerAgentIndexPerRound = agent.Index;

                    NextTurn();
                    AudioManager.Instance.PlaySfx(_cardPlayAudioClip);
                    // Debug.Log($"Point: {_currentPlayPoint}");

                    for (int i = 0; i < playCards.Count; i++)
                    {
                        playCards[i].UnClick();
                        playCards[i].gameObject.transform.SetParent(_middleCardDisplay);
                    }
                }
                else
                {
                    // Debug.Log("No valid evaluation card");
                }
            }
            else
            {
                int currentPlayCardType = (int)_currentCardPlayed;
                int evaluationPlayCardType = (int)_evaluationCardPlayed;

                if (currentPlayCardType >= 5 && evaluationPlayCardType >= 5)
                    isValidToCheckPoint = evaluationPlayCardType >= currentPlayCardType;
                else
                    isValidToCheckPoint = currentPlayCardType == evaluationPlayCardType;
            }

            // Debug.Log($"Is Valid To Check: {isValidToCheckPoint}");

            if (isValidToCheckPoint)
            {
                var evaluatePoint = point;
                int currentSuit = _currentPlayPoint / 1000;
                int currentNumber = _currentPlayPoint % 1000;

                int otherSuit = evaluatePoint / 1000;
                int otherNumber = evaluatePoint % 1000;

                bool checkifReplaceCard = otherNumber > currentNumber || (otherNumber == currentNumber && otherSuit > currentSuit);
                if (checkifReplaceCard)
                {
                    // Debug.Log("Replace cards!");
                    _currentPlayPoint = evaluatePoint;
                    _choosenAgent = agent;
                    _winnerAgentIndexPerRound = agent.Index;

                    for (int i = 0; i < playCards.Count; i++)
                        playCards[i].gameObject.transform.SetParent(_middleCardDisplay);
                    
                    AudioManager.Instance.PlaySfx(_cardPlayAudioClip);
                    _currentTotalCard = playCards.Count;
                    NextTurn();
                }
                else
                    AudioManager.Instance.PlaySfx(_cardInCorrectAudioClip);

                isReplaceCard = checkifReplaceCard;
                for (int i = 0; i < playCards.Count; i++)
                    playCards[i].UnClick();
            }
            else
            {
                for (int i = 0; i < playCards.Count; i++)
                    playCards[i].UnClick();
            }

            playCards.Clear();
            return isReplaceCard;
        }

        private bool CheckCardsIfPair(ref List<CardView> cards, out int point)
        {
            int totalCard = cards.Count;
            point = 0;
            if (totalCard < 2 || totalCard > 2)
                return false;

            DatabaseSO.CardType highestCardType = DatabaseSO.CardType.Diamond;
            for (int i = 0; i < 2; i++)
            {
                if (highestCardType < cards[i].Card.cardType)
                    highestCardType = cards[i].Card.cardType;
            }

            bool isValid = cards[0].Card.value == cards[1].Card.value;
            if (isValid)
            {
                int value = cards[0].Card.value == 2 ? 17 : cards[0].Card.value;
                point = (int)highestCardType * 1000 + value;
            }

            return isValid;
        }

        private bool CheckCardsIfTriple(ref List<CardView> cards, out int point)
        {
            int totalCard = cards.Count;
            point = 0;
            if (totalCard < 3 || totalCard > 3)
                return false;

            DatabaseSO.CardType highestCardType = DatabaseSO.CardType.Diamond;
            for (int i = 0; i < 3; i++)
            {
                if (highestCardType < cards[i].Card.cardType)
                    highestCardType = cards[i].Card.cardType;
            }

            bool isValid = cards[0].Card.value == cards[1].Card.value && cards[0].Card.value == cards[2].Card.value;
            if (isValid)
            {
                int value = cards[0].Card.value == 2 ? 17 : cards[0].Card.value;
                point = (int)highestCardType * 1000 + value;
            }

            return isValid;
        }

        private bool CheckCardsIfStraight(ref List<CardView> playCards, out int point)
        {
            point = 0;
            int validCount = 0;
            int startIndex = 0;
            int startValue = playCards[startIndex].Card.value;
            int lastValue = startValue;
            HashSet<int> hashInt = new HashSet<int>();

            for (int i = 0; i < playCards.Count; i++)
                hashInt.Add(playCards[i].Card.value);

            if (hashInt.Count < 5)
                return false;

            for (int i = 0; i < 5; i++)
            {
                bool res = hashInt.Contains(startValue);
                if (res)
                {
                    validCount += 1;
                }
                else
                {
                    startValue = lastValue;
                    break;
                }

                lastValue = startValue;
                startValue += 1;
                if (startValue >= 14)
                    startValue = 2;
            }

            if (validCount < 5)
            {
                validCount = 0;
                for (int i = 0; i < 5; i++)
                {
                    bool res = hashInt.Contains(startValue);
                    if (res)
                    {
                        validCount += 1;
                    }

                    startValue -= 1;

                    if (startValue < 2)
                        startValue = 14;
                }
            }

            bool isValid = validCount == 5;
            if (isValid)
                point = (int)playCards[4].Card.cardType * 1000 + (int)playCards[4].Card.value;

            return isValid;
        }

        private bool CheckCardIfFlush(ref List<CardView> playCards, out int point)
        {
            point = 0;
            if (playCards.Count < 5)
                return false;

            DatabaseSO.CardType cardType = playCards[0].Card.cardType;
            int validCount = 1;
            for (int i = 1; i < playCards.Count; i++)
            {
                if (playCards[i].Card.cardType == cardType)
                    validCount += 1;
            }

            bool isValid = validCount == 5;
            if (isValid)
                point = (int)playCards[4].Card.cardType * 1000 + playCards[4].Card.value;

            return isValid;
        }

        private bool CheckCardIfFullHouse(ref List<CardView> playCards, out int point)
        {
            point = 0;
            if (playCards.Count < 5)
                return false;

            Dictionary<int, int> tableCheck = new Dictionary<int, int>();
            List<int> key = new List<int>();
            for (int i = 0; i < playCards.Count; i++)
            {
                DatabaseSO.CardData card = playCards[i].Card;
                if (!tableCheck.ContainsKey(card.value))
                {
                    tableCheck.Add(card.value, 1);
                    key.Add(card.value);
                }
                else
                    tableCheck[card.value] += 1;
            }

            if (key.Count > 2)
                return false;

            bool isValid = tableCheck[key[0]] == 2 && tableCheck[key[1]] == 3 || (tableCheck[key[0]] == 3 && tableCheck[key[1]] == 2);
            if (isValid)
            {
                int maxCount = int.MinValue;
                int value = 0;
                foreach (var item in tableCheck)
                {
                    if (maxCount < item.Value)
                    {
                        maxCount = item.Value;
                        value = item.Key;
                    }
                }

                point = value * maxCount;
            }
            return isValid;
        }

        private bool CheckCardIfFourOfKinds(ref List<CardView> playCards, out int point)
        {
            point = 0;
            if (playCards.Count < 5)
                return false;

            Dictionary<int, int> tableCheck = new Dictionary<int, int>();
            List<int> key = new List<int>();
            for (int i = 0; i < playCards.Count; i++)
            {
                DatabaseSO.CardData card = playCards[i].Card;
                if (!tableCheck.ContainsKey(card.value))
                {
                    tableCheck.Add(card.value, 1);
                    key.Add(card.value);
                }
                else
                    tableCheck[card.value] += 1;
            }

            if (key.Count > 2)
                return false;

            bool isValid = tableCheck[key[0]] == 4 && tableCheck[key[1]] == 1 || (tableCheck[key[0]] == 1 && tableCheck[key[1]] == 4);
            if (isValid)
            {
                int maxCount = int.MinValue;
                int value = 0;
                foreach (var item in tableCheck)
                {
                    if (maxCount < item.Value)
                    {
                        maxCount = item.Value;
                        value = item.Key;
                    }
                }
                point = value;
            }
            return isValid;
        }

        private void GenerateCardDictionary()
        {
            if(_cardDictionary != null && _cardList != null)
                return;

            _cardDictionary = new Dictionary<int, DatabaseSO.CardData>();
            _cardList = new List<DatabaseSO.CardData>();
            int multiplier = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 3; j < 16; j++)
                {
                    DatabaseSO.CardData cardData = new DatabaseSO.CardData();
                    cardData.value = j == 15 ? 2 : j;
                    cardData.totalValue = j + (multiplier * 10);
                    cardData.cardType = (DatabaseSO.CardType)multiplier;
                    cardData.cardDatabseIndex = multiplier * 1000 + cardData.value;
                    string cardName = string.Empty;
                    switch (j)
                    {
                        case 11:
                            cardName = "J";
                            break;
                        case 12:
                            cardName = "Q";
                            break;
                        case 13:
                            cardName = "K";
                            break;
                        case 14:
                            cardName = "A";
                            break;
                        default:
                            cardName = cardData.value.ToString();
                            break;
                    }

                    switch (i)
                    {
                        case 0:
                            cardName = cardName + "d";
                            break;

                        case 1:
                            cardName = cardName + "c";
                            break;

                        case 2:
                            cardName = cardName + "h";
                            break;

                        case 3:
                            cardName = cardName + "s";
                            break;
                    }


                    cardData.cardName = cardName;
                    
                    _cardList.Add(cardData);
                    _cardDictionary.Add(cardData.totalValue, cardData);
                }
                multiplier = (int)Mathf.Pow(2, i + 1);
            }
        }

        private IEnumerator SpawnCardCoroutine(System.Action OnComplete = null)
        {
            List<CardView> listCardViews = new List<CardView>();
            AudioManager.Instance.PlaySfx(_cardShuffleAudioClip);
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < _cardList.Count; i++)
            {
                var cardView = ObjectPool.Instance.Spawn(_cardViewTemplateId.id).GetComponent<CardView>();
                cardView.transform.localPosition = Vector3.zero;
                cardView.transform.localScale = Vector3.one;
                cardView.Initialize(_cardList[i], _database);
                cardView.gameObject.SetActive(true);
                listCardViews.Add(cardView);
                yield return null;
            }

            int totalCard = listCardViews.Count;
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < agents.Length; j++)
                {
                    int idx = UnityEngine.Random.Range(0, totalCard);
                    agents[j].GiveCard(listCardViews[idx]);
                    listCardViews.RemoveAt(idx);
                    totalCard -= 1;
                    yield return null;
                }
            }

            OnComplete?.Invoke();
        }

        private IEnumerator Delay(System.Action onComplete)
        {
            yield return new WaitForSeconds(1f);
            onComplete?.Invoke();
        }

        private void ShowResult(string agentName)
        {
             _state = GameController.State.End;
             _resultController.Show(agentName);
        }

        public void Restart()
        {
            _currentPlayPoint = 0;
            _currentCardPlayed = CardPlayed.None;
            _restartCount += 1;
            ClearMiddleCards();
            _resultController.Hide();
            for (int i = 0; i < agents.Length; i++)
            {
                agents[i].ResetState();
            }
            StartGame();
        }

        private void ClearMiddleCards()
        {
            for (int i = _middleCardDisplay.childCount - 1; i >=0 ; i--)
            {
                ObjectPool.Instance.UnSpawn(_middleCardDisplay.GetChild(i).gameObject);
            }
        }
    }
}
