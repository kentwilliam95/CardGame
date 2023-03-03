using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Pusoy
{
    [CreateAssetMenu(menuName = "Asset/Card", fileName = "Card Data")]
    public class CardSO : ScriptableObject
    {
        [field: SerializeField] public Sprite Sprite { get; private set; }
    }
}