using Managers;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Util;

<<<<<<< Updated upstream
public class BaseMonster : MonoBehaviourPun, IPunObservable
=======
public class BaseMonster : NetworkUnit, IPunObservable, IDamageable
>>>>>>> Stashed changes
{
    [SerializeField] private Rigidbody2D rb;

    [Header("Patrol")]
    [SerializeField] private float distance;
    private Vector2[] points = new Vector2[4];
    private Coroutine moveRoutine;

    [Header("Stat")]
    [SerializeField] private MonsterStat stat;

    private void Start()
    {
        SetupPoints();

        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private void OnEnable()
    {
        if (points[0] != Vector2.zero && moveRoutine == null)
            StartCoroutine(MoveRoutine());
    }

    private void OnDisable()
    {
        moveRoutine = null;
    }

    private void SetupPoints()
    {
        Vector2 origin = transform.position;

        points[0] = origin + new Vector2(0, distance);
        points[1] = origin + new Vector2(distance, distance);
        points[2] = origin + new Vector2(distance, 0);
        points[3] = origin + new Vector2(0, 0);
    }

    private IEnumerator MoveRoutine()
    {
        int index = 0;

        while (true)
        {
            Vector2 target = points[index];

            while (Vector2.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    target,
                    4 * Time.deltaTime
                );

                yield return null;
            }

            index = (index + 1) % points.Length; // 0→1→2→3→0 순환

            //Test
            if (index == 3)
                Manager.Resources.Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if(stream.IsReading)
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }

    public void TakeDamage(float damage)
    {
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC(nameof(TakeDamage_RPC), RpcTarget.AllViaServer, damage);
    }

    [PunRPC]
    public void TakeDamage_RPC(float damage)
    {
        
    }
}
