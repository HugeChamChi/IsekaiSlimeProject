using Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Util;

public class UtilTest : MonoBehaviour
{
    private PhotonView pv;

    [SerializeField] private float range;
    [SerializeField] private OverlapType overlapType;
    [SerializeField] private float boxAngle;
    [SerializeField] private Vector2 boxsize;
    [SerializeField] private LayerMask targetLayer;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            Debug.Log(Utils.FindClosestTarget(transform.position, range, overlapType, targetLayer, boxAngle, boxsize, Filter).gameObject.name);
    }

    private bool Filter(GameObject obj)
    {
        return transform.position.y > 1;
    }    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, boxsize);
    }
}
