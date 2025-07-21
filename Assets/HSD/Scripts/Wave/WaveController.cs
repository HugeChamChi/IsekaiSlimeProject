using Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

[System.Serializable]
public struct WaveData
{
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

    public UI_Controller uiController;
    public WaveInfo Info;

    [Header("Wave")]
    [SerializeField] private WaveData[] waveDatas;
    private int curSpawnCount;
    private int curBossCount;
    private Coroutine waveRoutine;
    private Coroutine waveTimerRoutine;
    private Coroutine nextWaveRoutine;
    private WaitUntil clearCondition;

    [SerializeField] private GameObject prefab;
    
    private const int maxCount = 100;

    private void Awake()
    {
        waveDatas = Manager.Data.WaveDatas;
    }

    private void Start()
    {
        if(pv.IsMine)
        {
            MonsterStatusController.OnDied              += MonsterDie;
            MonsterStatusController.OnBossMonsterDied   += BossMonsterDie;
            
            uiController = new UI_Controller();
            Info = new();
            uiController.Init(this);

            clearCondition = new WaitUntil(() => Info.MonsterCount.Value == 0);
        }
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

            if (waveTimerRoutine == null)
                waveTimerRoutine = StartCoroutine(WaveTimerRoutine());
        }
    }    

    private IEnumerator WaveRoutine()
    {
        WaveData waveData = waveDatas[Info.CurWaveIdx.Value];
        waveData.WaveTime += 3;
        Info.WaveTimer.Value = waveData.WaveTime;
        curSpawnCount = 0;
        curBossCount = waveDatas[Info.CurWaveIdx.Value].SpawnBossCount;

        yield return Utils.GetDelay(3f);

        while (waveData.SpawnCount != curSpawnCount)
        {
            MonsterSpawn($"Monster/{waveData.SpawnMonster}");
            yield return Utils.GetDelay(waveData.SpawnTime / waveData.SpawnCount);
        }

        if(curBossCount > 0)
        {
             MonsterSpawnBossMonster($"Monster/{waveData.SpawnBoss}");            
        }

        if(nextWaveRoutine != null)
        {
            StopCoroutine(nextWaveRoutine);
            nextWaveRoutine = null;
        }
        nextWaveRoutine = StartCoroutine(NextWave());

        yield return Utils.GetDelay(waveData.WaveTime - waveData.SpawnTime - 3f);

        if(Info.CurWaveIdx.Value == waveDatas.Length - 1)
        {
            // 게임 클리어
        }
        else
        {
            Info.CurWaveIdx.Value++;
            WaveStart();
        }
    }

    private IEnumerator WaveTimerRoutine()
    {
        while(true)
        {
            Info.WaveTimer.Value -= Time.deltaTime;
            yield return null;
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

        Info.MonsterCount.Value--;
        // 골드추가 
    }

    private void BossMonsterDie(PhotonView pv, MonsterStat stat)
    {
        if (!pv.IsMine) return;

        uiController.BossClearPanel.Show(stat);
    }

    private void MonsterSpawn(string monsterName)
    {
        Manager.Resources.NetworkInstantiate<GameObject>(monsterName, transform.position, false, true);
        MonsterCountAddAndGameOverCheck();
    }

    private void MonsterCountAddAndGameOverCheck()
    {
        Info.MonsterCount.Value++;
        curSpawnCount++;
        Debug.Log(Info.MonsterCount.Value);
        if (Info.MonsterCount.Value >= maxCount)
        {
            // 게임 오버
        }
    }

    private void MonsterSpawnBossMonster(string monsterName)
    {
        GameObject boss = Manager.Resources.NetworkInstantiate<GameObject>(monsterName, transform.position, false, true);        

        MonsterCountAddAndGameOverCheck();

        uiController.BossAppearsPanel.Show(boss.GetComponent<MonsterStat>());
    }
    private void OnDestroy()
    {
        if (pv.IsMine)
            MonsterStatusController.OnDied -= MonsterDie;
    }
}
