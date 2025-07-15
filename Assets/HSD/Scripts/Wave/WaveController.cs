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
    public int WaveCount;
    public int WaveIdx;
    public float WaveTime;
    public float SpawnTime;
    public ClearEventType EventType;
    public string SpawnMonster;
    public string SpawnBoss;
    public int SpawnCount;
    public int SpawnBossCount;
}

public class WaveController : MonoBehaviour
{
    [SerializeField] private PhotonView pv;

    [Header("Wave")]
    [SerializeField] private WaveData[] waveDatas;
    private int curWaveIdx;
    private int curSpawnCount;
    private int curBossCount;
    private Coroutine waveRoutine;
    private Coroutine nextWaveRoutine;
    private WaitUntil clearCondition;

    [SerializeField] private GameObject prefab;

    [Header("")]
    private const int maxCount = 100;
    private int monsterCount;

    private void Start()
    {
        clearCondition = new WaitUntil(() => monsterCount == 0);

        if(pv.IsMine)
            MonsterStatusController.OnDied += MonsterDie;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            pv.RPC(nameof(WaveStart_RPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    private void WaveStart_RPC()
    {
        if(pv.IsMine)
            WaveStart();
    }

    private void WaveStart()
    {   
        if(pv.IsMine)
        {
            if(waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }
            waveRoutine = StartCoroutine(WaveRoutine());
        }
    }

    private IEnumerator WaveRoutine()
    {
        WaveData waveData = waveDatas[curWaveIdx];        
        curSpawnCount = 0;
        curBossCount = waveDatas[curWaveIdx].SpawnBossCount;

        yield return Utils.GetDelay(3f);

        while (waveData.SpawnCount != curSpawnCount)
        {
            MonsterSpawn(waveData.SpawnMonster);
            yield return Utils.GetDelay(waveData.SpawnTime / waveData.SpawnCount);
        }

        if(curBossCount > 0)
            MonsterSpawn(waveData.SpawnBoss);

        if(nextWaveRoutine != null)
        {
            StopCoroutine(nextWaveRoutine);
            nextWaveRoutine = null;
        }
        nextWaveRoutine = StartCoroutine(NextWave());

        yield return Utils.GetDelay(waveData.WaveTime - waveData.SpawnTime);

        if(curWaveIdx == waveDatas.Length - 1)
        {
            // 게임 클리어
        }
        else
        {
            curWaveIdx++;
            WaveStart();
        }        
    }

    private IEnumerator NextWave()
    {
        yield return clearCondition;

        WaveStart();
    }

    private void MonsterDie(PhotonView pv)
    {
        if (!pv.IsMine) return;

        monsterCount--;
    }

    private void MonsterSpawn(string monsterName)
    {
        Manager.Resources.NetworkInstantiate<GameObject>(monsterName, transform.position, false, true);
        monsterCount++;
        curSpawnCount++;

        if(monsterCount >= maxCount)
        {
            // 게임 오버
        }
    }
}
