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
}
