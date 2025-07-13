using Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Util;

//WaveIndex

//Stage

//Wave

//WaveTime // 웨이브 진행시간

//SpawnTime // 몬스터가 소환되는 총 시간

//WaveGold // 몬스터에서 획득가능한 골드 량

//Message // 웨이브 클리어 시 호출되는 이벤트 결정을 Enum으로 이벤트 관리

//Monster // 스폰될 몬스터

//Boss // 스폰될 보스

//SpawnCount // 스폰될 몬스터의 수량

//Interval = SpawnTime / SpawCount;

//SpawnBossCount // 스폰될 보스 수

//SpawnHP // 해당 웨이브 몬스터 당 체력

[System.Serializable]
public struct WaveData
{
    public int WaveIdx;
    public int Wave;
    public float WaveTime;
    public float SpawnTime;
    public int WaveGold;
    public ClearEventType EventType;
    public string SpawnMonster;
    public string SpawnBoss;
    public int SpawnCount;
    public int SpawnBossCount;
    public float SpawnHP;
}

public class WaveController : MonoBehaviour
{
    [SerializeField] private PhotonView pv;

    [Header("Wave")]
    [SerializeField] private WaveData[] waveDatas;
    private int curWaveIdx;
    private int curSpawnCount;
    private int curBossCount;
    private bool isClear;
    private Coroutine waveRoutine;

    [SerializeField] private GameObject testPrefab;

    private void Start()
    {
        WaveStart();
        PhotonNetwork.ConnectUsingSettings();
    }    

    private void WaveStart()
    {        
        waveRoutine = StartCoroutine(WaveRoutine());
    }

    private void WaveClear()
    {
        curBossCount--;

        if (curBossCount <= 0)
            isClear = true;

        // 클리어 이벤트 실행
    }

    private IEnumerator WaveRoutine()
    {
        curSpawnCount = 0;
        curBossCount = waveDatas[curWaveIdx].SpawnBossCount;
        isClear = false;

        while (waveDatas[curWaveIdx].SpawnCount != curSpawnCount)
        {
            Manager.Resources.NetworkInstantiate(testPrefab, transform.position);
            curSpawnCount++;
            yield return Utils.GetDelay(waveDatas[curWaveIdx].SpawnTime / waveDatas[curWaveIdx].SpawnCount);
        }

        Manager.Resources.NetworkInstantiate(testPrefab, transform.position);
        yield return Utils.GetDelay(waveDatas[curWaveIdx].WaveTime - waveDatas[curWaveIdx].SpawnTime);

        if(isClear)
        {
            WaveClear();
        }
        else
        {
            // 게임 오버
        }
    }
}
