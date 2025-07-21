using Managers;
using Photon.Pun;
using PlayerField;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;


public class BaseMonster : NetworkUnit
{
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    [Header("Patrol")]
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

        foreach (Vector2 point in points)
        {
            Debug.Log(point);
        }

        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private void OnEnable()
    {
        if (points[0] != Vector2.zero && moveRoutine == null)
        {
            index = 0;
            StartCoroutine(MoveRoutine());
        }

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
            PlayerFieldManager.Instance.GetLocalFieldController().MapSlot[0].SpawnPosition,
            PlayerFieldManager.Instance.GetLocalFieldController().MapSlot[5].SpawnPosition,
            PlayerFieldManager.Instance.GetLocalFieldController().MapSlot[35].SpawnPosition,
            PlayerFieldManager.Instance.GetLocalFieldController().MapSlot[30].SpawnPosition
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
            Debug.Log($"[MoveRoutine] INDEX - {index}");
            Vector2 target = points[index];

            Vector2 direction = (target - (Vector2)transform.position).normalized;
            Debug.Log($"[MoveRoutine] direction - {direction}");
            sr.flipX = direction.x < 0;

            while (Vector2.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    target,
                    status.Speed.Value * Time.deltaTime
                );

                yield return null;
            }

            index = (index + 1) % points.Length; // 0→1→2→3→0 순환
        }
    }
}
