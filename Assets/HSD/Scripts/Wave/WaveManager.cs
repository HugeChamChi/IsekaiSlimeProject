using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class WaveManager
{
        
}

[System.Serializable]
public class PlayerWaveStatus
{
    public int currentWaveIdx = 0;
    public bool isWaveActive = false;
    public int enemiesAlive = 0;
    public int clearedWaves = 0;
    public bool isEliminated = false;
}
