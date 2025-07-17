using Managers;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Util;

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
            
            //필드 매니저에 플레이어의 PlayerFieldController 등록 (추가)
            PlayerFieldManager.Instance.RegisterPlayerField(players[i].ActorNumber, map.fieldController);
        }
    }

    private void MapSetting()
    {
        int idx = 0;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == PhotonNetwork.LocalPlayer)
            {
                Camera mapCam = maps[players[i]].Cam;
                mapCam.depth = 1;
                mapCam.AddComponent<AudioListener>();
                var camRay = mapCam.AddComponent<CameraRay>(); //local player의 map에만 camera ray 추가
                camRay.SetCamera(mapCam);  // 카메라 지정
                
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
