using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStat : ScriptableObject
{
    public Stat<float, float> Health = new Stat<float, float>((a,b) => a+b, value => value);
    public Stat<float, float> Speed = new Stat<float, float>((a, b) => a + b, value => value);
    public Stat<float, float> Defense = new Stat<float, float>((a, b) => a + b, value => value / 100);    
}
