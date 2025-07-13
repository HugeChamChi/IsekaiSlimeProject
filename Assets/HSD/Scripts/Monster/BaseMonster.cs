using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    private Vector2 moveDir;


    [SerializeField] private float moveSpeed;    

    private IEnumerator MoveRoutine()
    {
        moveDir = Vector2.up;

        while (true)
        {
            yield return null;
        }
    }
}
