using System.Collections.Generic;
using UnityEngine;

namespace RPG.Stats
{
    public interface IModifierProvider
    {
        IEnumerable<int> GetAdditiveModifiers(Stat stat);
        IEnumerable<int> GetPercentageModifiers(Stat stat);
    }
}

