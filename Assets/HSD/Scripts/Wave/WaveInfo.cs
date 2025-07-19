using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveInfo
{
    public Property<int> CurWaveIdx = new();
    public Property<int> MonsterCount = new();
    public Property<float> WaveTimer = new();
}
