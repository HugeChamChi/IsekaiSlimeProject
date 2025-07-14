using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private GameObject mapPrefab;
    [SerializeField] private RenderTexture[] playTextures;
    private Player[] players;

    private static readonly Dictionary<Player, Map> maps = new Dictionary<Player, Map>();

    [SerializeField] private float xInterval;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            players = PhotonNetwork.PlayerList;
            MapGenerate();
            MapSetting();
        }
    }
    private void Start()
    {
        
    }

    private void MapGenerate()
    {
        Map map = null;        

        for (int i = 0; i < players.Length; i++)
        {
            map = Instantiate(mapPrefab, new Vector3(xInterval * i, 0), Quaternion.identity).GetComponent<Map>();
            map.Owner = players[i];
            maps.Add(players[i], map);
        }
    }

    private void MapSetting()
    {
        int idx = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == PhotonNetwork.LocalPlayer)
            {
                maps[players[i]].Cam.depth = 1;
                maps[players[i]].Cam.AddComponent<AudioListener>();
                maps[players[i]].CreateWaveController();
            }
            else
            {
                maps[players[i]].Cam.targetTexture = playTextures[idx];
                maps[players[i]].Cam.depth = 0;
                idx++;
            }
        }
    }
}
