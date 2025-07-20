using Managers;
using Photon.Pun;
using Photon.Realtime;
using PlayerField;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class Map : MonoBehaviour
{
    public Camera Cam;

    [Header("Info")]
    public Player Owner;

    [Header("Wave")]
    private const string waveController = "Prefabs/WaveController";
    [SerializeField] private Vector2 spawnPoint;

    [Header("PlayerFieldController")] 
    public PlayerFieldController fieldController; 
    
    public void CreateWaveController()
    {
        spawnPoint = PlayerFieldManager.Instance.GetLocalFieldController().MapSlot[0].SpawnPosition;
        PhotonNetwork.Instantiate(waveController, spawnPoint, Quaternion.identity);
    }
}
