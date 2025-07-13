using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public interface IEffectProvider
{
    void Execute(IEffectReceiver receiver);    
}

[System.Serializable]
public struct StatEffect<T>
{
    public StatType statType;
    public T Value;
    public float Duration;
}

public class UnitTestStat_HSD : ScriptableObject, IEffectProvider
{
    public string unitName;
    public StatEffect<float> StatEffect { get; set; }

    public void Execute(IEffectReceiver receiver)
    {
        receiver.ReceiveEffect(StatEffect.statType, StatEffect.Value, StatEffect.Duration, $"{unitName}_{StatEffect.statType.ToString()}");
    }
}

public class MonsterTest : MonoBehaviour, IEffectReceiver // �̰� �³�...?
{    
    public MonsterStat stat {  get; private set; }
    private Dictionary<string, Coroutine> activeEffects = new Dictionary<string, Coroutine>();

    public void ReceiveEffect(StatType type, float amount, float duration, string source)
    {
        if (activeEffects.ContainsKey(source))
        {
            StopCoroutine(activeEffects[source]);
        }

        activeEffects[source] = StartCoroutine(EffectRoutine(type, amount, duration, source));
    }

    public IEnumerator EffectRoutine(StatType type, float amount, float duration, string source)
    {
        switch (type)
        {
            case StatType.Health:
                if (stat.Health.CheckDuplication(source))
                {
                    stat.Health.RemoveModifierAll(source);
                }
                stat.Health.AddModifier(amount, source);
                break;
            case StatType.Speed:
                if (stat.Speed.CheckDuplication(source))
                {
                    stat.Speed.RemoveModifierAll(source);
                }
                stat.Speed.AddModifier(amount, source);
                break;
            case StatType.Defense:
                if (stat.Defense.CheckDuplication(source))
                {
                    stat.Defense.RemoveModifierAll(source);
                }
                stat.Defense.AddModifier(amount, source);
                break;
        }

        yield return Utils.GetDelay(duration);

        switch (type)
        {
            case StatType.Health:
                stat.Health.RemoveModifierAll(source);
                break;
            case StatType.Speed:
                stat.Speed.RemoveModifierAll(source);
                break;
            case StatType.Defense:
                stat.Defense.RemoveModifierAll(source);
                break;
        }

        activeEffects.Remove(source);
    }

}