using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStat", menuName = "Monster/Data")]
public class MonsterStat : ScriptableObject
{
    public int ID;
    public string Name;
    public float Hp;
    public float moveSpeed;
    public float defense;
    public int dropGold;    
}
