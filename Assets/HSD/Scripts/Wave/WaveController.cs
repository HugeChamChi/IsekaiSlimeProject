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

//WaveTime // ���̺� ����ð�

//SpawnTime // ���Ͱ� ��ȯ�Ǵ� �� �ð�

//WaveGold // ���Ϳ��� ȹ�氡���� ��� ��

//Message // ���̺� Ŭ���� �� ȣ��Ǵ� �̺�Ʈ ������ Enum���� �̺�Ʈ ����

//Monster // ������ ����

//Boss // ������ ����

//SpawnCount // ������ ������ ����

//Interval = SpawnTime / SpawCount;

//SpawnBossCount // ������ ���� ��

//SpawnHP // �ش� ���̺� ���� �� ü��

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

        // Ŭ���� �̺�Ʈ ����
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
            // ���� ����
        }
    }
}
