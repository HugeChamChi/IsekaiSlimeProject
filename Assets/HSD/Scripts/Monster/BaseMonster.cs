using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;


public class BaseMonster : NetworkUnit, IPunObservable
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Patrol")]
    [SerializeField] private float distance;
    private Vector2[] points = new Vector2[4];
    private Coroutine moveRoutine;

    [Header("Status")]
    private MonsterStatusController status;
    [Space]

    private int index = 0;

    private void Start()
    {
        sr      = GetComponent<SpriteRenderer>();
        rb      = GetComponent<Rigidbody2D>();
        status  = GetComponent<MonsterStatusController>();

        SetupPoints();

        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private void OnEnable()
    {
        //if (points[0] != Vector2.zero && moveRoutine == null)
        //    StartCoroutine(MoveRoutine());
        status ??= GetComponent<MonsterStatusController>();
        status.isFaint.AddEvent(Faint);
    }

    private void OnDisable()
    {
        moveRoutine = null;

        status.isFaint.RemoveEvent(Faint);
    }

    private void SetupPoints()
    {
        Vector2 origin = transform.position;

        points = new Vector2[]
        {
            origin + new Vector2(distance, 0),
            origin + new Vector2(distance, -distance),
            origin + new Vector2(0, -distance),
            origin + new Vector2(0, 0)
        };
    }

    private void Faint(bool isFaint)
    {
        if (isFaint)
            StopMove();
        else
            ReStartMove();
    }

    private void StopMove()
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
    }

    private void ReStartMove()
    {
        if(moveRoutine == null)
            moveRoutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {       
        while (true)
        {
            Vector2 target = points[index];

            Vector2 direction = (target - (Vector2)transform.position).normalized;

            sr.flipX = direction.x < 0;

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
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else if (stream.IsReading)
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
