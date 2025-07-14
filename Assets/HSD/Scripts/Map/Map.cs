using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public Camera Cam;

    [Header("Info")]
    public Player Owner;

    [Header("Wave")]
    private static readonly string waveController = "Prefabs/WaveController";
    [SerializeField] private Transform spawnPoint;

    public void CreateWaveController()
    {
        PhotonNetwork.Instantiate(waveController, spawnPoint.position, spawnPoint.rotation);
    }
}
