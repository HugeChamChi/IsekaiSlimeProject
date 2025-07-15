using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStatusController : MonoBehaviour, IDamageable
{
    [Header("Stat")]
    [SerializeField] private MonsterStat baseStat;
    public Stat<float, float> Health = new Stat<float, float>((a, b) => a + b, value => value);
    public Stat<float, float> Speed = new Stat<float, float>((a, b) => a + b, value => value);
    public Stat<float, float> Defense = new Stat<float, float>((a, b) => a + b, value => value / 100);
    public Stat<float, float> DefenseMultiply = new Stat<float, float>((a, b) => a + b, value => value);
    public Property<float> CurHp;

    private PhotonView pv;

    public static event Action<PhotonView> OnDied;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
        SetupStat();
    }

    private void SetupStat()
    {
        Health.SetBaseValue(baseStat.Hp);
        Speed.SetBaseValue(baseStat.defense);
        Defense.SetBaseValue(baseStat.defense);
    }

    public void TakeDamage(float damage)
    {
        if (!pv.IsMine) return;

        float defence = DefenseMultiply.Value == 0 ? 1 - Defense.Value : 1 - ((1 + DefenseMultiply.Value / 100) * Defense.Value);

        damage *= defence;

        CurHp.Value -= damage;

        if (CurHp.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke(pv);

        if(pv.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }
}
