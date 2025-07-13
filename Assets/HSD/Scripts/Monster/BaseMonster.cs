using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class BaseMonster : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;    
    [SerializeField] private float moveSpeed;

    [SerializeField] private float distance;
    Vector2[] points;

    private void Start()
    {
        StartCoroutine(MoveRoutine());
    }

    private void SetupPoints()
    {
        for (int i = 0; i < points.Length; i++)
        { 
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            while (Vector2.Distance(transform.position, points[0]) > .1f)
            {
                Vector2.MoveTowards(transform.position, points[0], moveSpeed * Time.deltaTime);
                yield return null;
            }

            while (Vector2.Distance(transform.position, points[1]) > .1f)
            {
                Vector2.MoveTowards(transform.position, points[1], moveSpeed * Time.deltaTime);
                yield return null;
            }

            while (Vector2.Distance(transform.position, points[2]) > .1f)
            {
                Vector2.MoveTowards(transform.position, points[2], moveSpeed * Time.deltaTime);
                yield return null;
            }

            while (Vector2.Distance(transform.position, points[3]) > .1f)
            {
                Vector2.MoveTowards(transform.position, points[3], moveSpeed * Time.deltaTime);
                yield return null;
            }
        }        
    }
}
