using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using UnityEngine;
using UnityEngine.Networking;
using static UnityEngine.Rendering.DebugUI.Table;

public class DataDownloader
{
    private const string URL = "https://docs.google.com/spreadsheets/d/10QfD3I1AbOf_yOnV5AEE4zut2KNPh1pmiAEeALIUZzA/export?format=csv&range=B66:C66";
    // gid=2097254203 
    // 
    private const string MonsterURL = "https://docs.google.com/spreadsheets/d/16VV5EYBHec3ZFEOQceXpXT-rPAykr-poMBNdsq6h2FY/export?format=csv&gid=486541091&range=C5:I24";

    public event Action OnDataSetupCompleted;

    public IEnumerator DownloadData()
    {
        Debug.Log("진입");
        yield return null;
        yield return LoadCSV(MonsterURL, SetupWaveDatas);
        Debug.Log("끝");
        OnDataSetupCompleted?.Invoke();
    }

    private IEnumerator LoadCSV(string url, Action<string[][]> onParsed, int startLine = 1)
    {
        using UnityWebRequest www = UnityWebRequest.Get(url);

        yield return www.SendWebRequest();

        if (!string.IsNullOrEmpty(www.error))
            yield break;

        string raw = www.downloadHandler.text.Trim();
        string[] lines = raw.Split('\n');
        List<string[]> parsed = new();

        for (int i = startLine - 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Trim().Split(',');
            parsed.Add(row);
        }

        onParsed?.Invoke(parsed.ToArray());
    }

    private void SetupWaveDatas(string[][] data)
    {
        WaveData[] waveDatas = new WaveData[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            string[] row = data[i];
            waveDatas[i] = new WaveData
            {
                WaveTime = float.Parse(row[0]),
                SpawnTime = float.Parse(row[1]),
                EventType = Enum.Parse<ClearEventType>(row[2]),
                SpawnMonster = row[3],
                SpawnBoss = row[4],
                SpawnCount = int.Parse(row[5]),
                SpawnBossCount = int.Parse(row[6])
            };
        }
        Manager.Data.WaveDatas = waveDatas;
    }
}
