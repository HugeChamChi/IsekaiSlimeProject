using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkUnit : MonoBehaviourPun
{
    protected virtual void Awake()
    {
        Provider.Add<PhotonView>(gameObject);
    }

    protected virtual void OnDestroy()
    {
        Provider.Remove<PhotonView>(gameObject);
    }

    [PunRPC]
    public void RemoteSetInactive()
    {
        gameObject.SetActive(false);
    }
    [PunRPC]
    public void RemoteSetactive(Vector2 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
    }
}
