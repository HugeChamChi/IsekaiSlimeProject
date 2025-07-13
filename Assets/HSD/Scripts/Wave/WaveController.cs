using Managers;
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
    public int Stage;
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
    [SerializeField] private WaveData[] waveDatas;
    private int curWaveIdx;
    private int curSpawnCount;
    private int curBossCount;
    private Coroutine waveRoutine;

    [SerializeField] private GameObject testPrefab;

    private void Start()
    {
        WaveStart();
    }

    private void WaveStart()
    {        
        waveRoutine = StartCoroutine(WaveRoutine());
    }

    private void WaveClear()
    {
        curBossCount--;

        if(curBossCount <= 0)
            StopCoroutine(waveRoutine);
        
        // Ŭ���� �̺�Ʈ ����
    }

    private IEnumerator WaveRoutine()
    {
        curSpawnCount = 0;
        curBossCount = waveDatas[curWaveIdx].SpawnBossCount;

        while (waveDatas[curWaveIdx].SpawnCount != curSpawnCount)
        {
            Instantiate(testPrefab);
            //Manager.Resources.Instantiate<GameObject>(waveDatas[curWaveIdx].SpawnMonster, transform.position);
            curSpawnCount++;
            yield return Utils.GetDelay(waveDatas[curWaveIdx].SpawnTime / waveDatas[curWaveIdx].SpawnCount);
        }

        Instantiate(testPrefab);
        //Manager.Resources.Instantiate<GameObject>(waveDatas[curWaveIdx].SpawnBoss, transform.position);
        yield return Utils.GetDelay(waveDatas[curWaveIdx].WaveTime - waveDatas[curWaveIdx].SpawnTime);

        // ���� ����
    }
}
