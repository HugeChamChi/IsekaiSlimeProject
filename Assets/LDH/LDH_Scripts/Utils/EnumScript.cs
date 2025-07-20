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

namespace Card
{
    public enum CardType
    {
        AttackPowerUp, GoldGainUp, AllEnemyDefenseDown, AttackSpeedUp
    }
}
namespace Monster
{
    public enum MonsterType
    {
        Common, Boss
    }
}

public enum ClearEventType
{
    None, Effect
}


//임시 생성 (컴파일 오류 해결)
public enum StatType
{
    hp,
    speed,
    
}

namespace Unit
{
    public enum UnitTier { Common = 1, Rare, Epic, Legendary }
    public enum UnitType { RangeAttack = 1, Faint = 2, MoveSpeedBuff = 3 }
    public enum EffectType { None = 1, Faint, Slow }
    
    public enum SkillRangeType {Short1 = 1, Short2, Long1, Long2}
    
    public enum OutlineType {None, Skill, Select}
    
}

namespace PlayerField
{
    public enum SlotType
    {
        Inner,  // 유닛 소환 가능 영역
        Outer   // 테두리, 몬스터 이동, 스킬 표시용
    }
}