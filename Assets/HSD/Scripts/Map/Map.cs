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
    private static readonly string waveController = "Prefabs/WaveController";
    [SerializeField] private Transform spawnPoint;

    [Header("PlayerFieldController")] 
    public PlayerFieldController fieldController; 
    
    public void CreateWaveController()
    {
        PhotonNetwork.Instantiate(waveController, spawnPoint.position, spawnPoint.rotation);
    }
}
