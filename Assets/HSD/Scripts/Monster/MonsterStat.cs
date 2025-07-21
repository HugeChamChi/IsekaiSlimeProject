using Monster;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStat", menuName = "Monster/Data")]
public class MonsterStat : ScriptableObject
{
    public int ID;
    public string Name;
    public Sprite icon;
    public float Hp;
    public float MoveSpeed;
    public float Defense;
    public int DropGold;  
    public MonsterType MonsterType;
}
