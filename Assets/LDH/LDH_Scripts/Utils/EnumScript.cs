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



namespace Unit
{
    public enum UnitTier { Common, Rare, Epic }
    public enum UnitType { RangeAttack, SpeedDebuff, ArmorDebuff, AttackSpeedBuff, AttackPowerBuff }
    public enum EffectType { Faint, Slow }
    public enum ClearEventType
    {
        None, Effect
    }
}
