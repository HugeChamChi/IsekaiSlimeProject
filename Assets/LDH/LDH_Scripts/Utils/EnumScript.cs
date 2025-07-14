using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public enum OverlapType
    {
        Circle, Box
    }

}

public enum ClearEventType
{
    None, Effect
}


namespace Unit
{
    public enum UnitTier { Common, Rare, Epic, Legendary }
    public enum UnitType { RangeAttack, SpeedDebuff, ArmorDebuff, AttackSpeedBuff, AttackPowerBuff }
    public enum EffectType { Faint, Slow }
}
