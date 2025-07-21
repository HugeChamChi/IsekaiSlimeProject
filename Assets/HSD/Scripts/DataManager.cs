using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : DesignPattern.Singleton<DataManager>
{
    public WaveData[] WaveDatas;
    private DataDownloader dataDownloader;

    private void Start()
    {
        dataDownloader = new DataDownloader();
        StartCoroutine(dataDownloader.DownloadData());
    }
}
