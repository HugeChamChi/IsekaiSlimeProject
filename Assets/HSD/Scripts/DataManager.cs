using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : DesignPattern.Singleton<DataManager>
{
    public Property<int> Gold = new();
    public Property<int> Gem = new();

    public WaveData[] WaveDatas;
    public MonsterStat[] monsterStats;
    private DataDownloader dataDownloader;    

    private void Start()
    {
        monsterStats = Resources.LoadAll<MonsterStat>("Data/Monster");

        dataDownloader = new DataDownloader();
        StartCoroutine(dataDownloader.DownloadData());
    }
}
