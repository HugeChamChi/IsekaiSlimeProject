using System.Collections.Generic;
using Unit;
using UnityEngine;

//todo: IDamagable 수정
using IDamagable = LDH.LDH_Scripts.Temp.Temp_IDamagable;

namespace Units
{
    public class Unit : MonoBehaviour {
    public int Index;
    public string Name;
    public UnitTier Tier;
    public UnitType Type;
    public string Description;
    public string ModelFileName;

    public UnitStat Stat;
    public UnitSkill Skill;

    
    
    private void Awake() {
        // Stat = new UnitStat();
        // Skill = new();
    }

    public void Attack(IDamagable target) 
    {
        
    }

    public void UseSkill(int index, List<IDamagable> targets) {
        // Skill.Execute(this, targets);
    }
}

}
