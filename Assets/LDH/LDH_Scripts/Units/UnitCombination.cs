using System.Collections.Generic;
using Unit;
using UnityEngine;

namespace Units
{
    [System.Serializable]
    public class UnitCombination
    {
       public List<UnitCombinationEntry> Entries;
    }
    
    [System.Serializable]
    public class UnitCombinationEntry
    {
        public UnitTier Tier;  // 예: Epic
        public int UnitIndex;  // 예: 1 (슬라임)
        public int RequiredCount;  // 예: 3
    }
}