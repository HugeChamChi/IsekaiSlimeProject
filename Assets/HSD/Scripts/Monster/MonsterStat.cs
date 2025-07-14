using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : ScriptableObject
{
    public int ID;
    public string Name;
    public float Hp;
    public float moveSpeed;
    public float defense;
    public int dropGold;
    public int WaveIdx;

    // 테스트 용 안씀
    public Stat<float, float> Health = new Stat<float, float>((a,b) => a+b, value => value);
    public Stat<float, float> Speed = new Stat<float, float>((a, b) => a + b, value => value);
    public Stat<float, float> Defense = new Stat<float, float>((a, b) => a + b, value => value / 100);    
}
