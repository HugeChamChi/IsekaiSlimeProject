using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Managers;

public class TestNetwork : MonoBehaviourPunCallbacks
{
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnected();
        Debug.Log("���� ��");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("�� ��");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        PhotonNetwork.JoinOrCreateRoom("TestRoom", roomOptions, TypedLobby.Default);
        Manager.InitPool();
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("�� ����");
        if (PhotonNetwork.IsMasterClient)
            Debug.Log("������ Ŭ���̾�Ʈ �Դϴ�.");      
    }
}
