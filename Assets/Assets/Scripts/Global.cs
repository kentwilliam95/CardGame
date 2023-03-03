using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Pusoy
{
    public class Global
    {
        public static void SortPlayCards(ref List<CardView> playCards)
        {
            int smallest = int.MaxValue;
            int index = 0;
            List<CardView> sortedPlayCards = new List<CardView>();
            int totalLoop = playCards.Count;
            for (int j = 0; j < totalLoop; j++)
            {
                for (int i = 0; i < playCards.Count; i++)
                {
                    if (playCards[i].Card.value < smallest)
                    {
                        smallest = playCards[i].Card.value;
                        index = i;
                    }
                }

                sortedPlayCards.Add(playCards[index]);
                playCards.RemoveAt(index);
                smallest = int.MaxValue;
                index = 0;
            }

            playCards = sortedPlayCards;
            sortedPlayCards = null;
        }

        public static IEnumerator FadeIenumerator(float from, float to, float time, Action<float> onUpdate = null, Action onComplete = null)
        {
            float percentage = 0f;
            float speed = 1f / time;

            for (float t = 0; t <= 1f;)
            {
                onUpdate?.Invoke(Mathf.Lerp(from, to, t));
                percentage += Time.unscaledDeltaTime * speed;
                t += Time.unscaledDeltaTime * speed;
                yield return null;
            }

            onUpdate?.Invoke(Mathf.Lerp(from, to, percentage));
            onComplete?.Invoke();
        }
    }
}
