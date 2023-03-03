using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pusoy
{
    public interface IGiveCard
    {
        void GiveCard(CardView card);
        bool HaveThreeOfDiamond();
    }
}
