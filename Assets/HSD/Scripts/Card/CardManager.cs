using DesignPattern;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : Singleton<CardManager>
{
    [field:SerializeField] public Property<float> AttackPower { get; private set; }
    [field:SerializeField] public Property<float> AttackSpeed { get; private set; }
    [field:SerializeField] public Property<float> GoldGainUp { get; private set; }
    [field:SerializeField] public Property<float> AllEnemyDefenseDown { get; private set; }

    [field: SerializeField] private CardData[] cardDatas;

    private static readonly int[] weights = { 50, 25, 20, 5 };    

    private void Start()
    {
        Init();
        cardDatas = Resources.LoadAll<CardData>("Data/Card");
    }

    public void Init()
    {
        AttackPower = new Property<float>();
        AttackSpeed = new Property<float>();
        GoldGainUp = new Property<float>();
        AllEnemyDefenseDown = new Property<float>();

        AttackPower.Value = 1;
        AttackSpeed.Value = 1;
        GoldGainUp.Value = 1;
        AllEnemyDefenseDown.Value = 1;
    }

    public CardData GetRandomCardData()
    {
        int rand = Random.Range(0, 4);

        CardData[] randomCardDatas = cardDatas
            .Where(c => (int)c.effect.type == rand)
            .OrderByDescending(c => c.effect.amount)
            .ToArray();

        rand = Random.Range(0, 100);

        int select = 0;

        int cumulative = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative)
            {
                select = i;
                break;
            }
        }

        return randomCardDatas[select];
    }
}
