using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class UtilTest : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private OverlapType overlapType;
    [SerializeField] private float boxAngle;
    [SerializeField] private Vector2 boxsize;
    [SerializeField] private LayerMask targetLayer;

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
