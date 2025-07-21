using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit;
using UnityEngine;

public class MonsterStatusController : MonoBehaviour, IDamageable, IEffectable
{
    [Header("Stat")]
    public MonsterStat baseStat;
    public Stat<float, float> Health            = new Stat<float, float>((a, b) => a + b, value => value);
    public Stat<float, float> Speed             = new Stat<float, float>((a, b) => a + b, value => value);
    public Stat<float, float> Defense           = new Stat<float, float>((a, b) => a + b, value => value / 100);
    public Stat<float, float> DefenseMultiply   = new Stat<float, float>((a, b) => a + b, value => value);

    public Property<float> CurHp    = new();
    public Property<bool> isFaint   = new();

    private PhotonView pv;    

    public static event Action<PhotonView> OnDied;
    public static event Action<PhotonView, MonsterStat> OnBossMonsterDied;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        SetupStat();
    }

    private void SetupStat()
    {
        Health.SetBaseValue(baseStat.Hp);
        Speed.SetBaseValue(baseStat.Defense);
        Defense.SetBaseValue(baseStat.Defense);
        CurHp.Value = Health.Value;
    }

    public void TakeDamage(float damage)
    {
        if (!pv.IsMine) return;

        float defence = DefenseMultiply.Value == 0 ? 1 - Defense.Value : 1 - ((1 + DefenseMultiply.Value / 100) * Defense.Value);

        damage *= defence;
        
        Debug.Log($"[Monster] 몬스터한테 적용되는 최종 데미지 : {damage}");

        CurHp.Value -= damage;
        Debug.Log($"[Monster] current hp = {CurHp.Value}");

        if (CurHp.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke(pv);

        if(baseStat.MonsterType == Monster.MonsterType.Boss)
            OnBossMonsterDied?.Invoke(pv,baseStat);

        if(pv.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    public void Apply(EffectType type, float amount)
    {
        switch (type)
        {
            case EffectType.Faint:
                isFaint.Value = true;
                break;
            case EffectType.Slow:
                
                float slowAmount = Speed.Value * (amount / 100);
                Speed.AddModifier(slowAmount, "Slow");
                
                Debug.Log($"[Monster] 이속 적용 {amount} - 실제 스피드 {Speed.Value}");
                break;
        }
    }

    public void Revoke(EffectType type, float amount)
    {
        switch (type)
        {
            case EffectType.Faint:
                isFaint.Value = false;
                break;
            case EffectType.Slow:
                float slowAmount = Speed.Value * (amount / 100);
                Speed.RemoveModifier(slowAmount, "Slow");
                break;
        }
    }
}
