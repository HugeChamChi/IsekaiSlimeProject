using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEffect", menuName = "Card/Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea]
    public string description;
    public Sprite icon;
    public CardEffect effect;

    private void OnValidate()
    {
        if (effect == null) return;

        switch (effect.type)
        {
            case Card.CardType.AttackPowerUp:
                cardName = "공격력 증가";
                break;
            case Card.CardType.GoldGainUp:
                cardName = "획득 재화량 증가";
                break;
            case Card.CardType.AllEnemyDefenseDown:
                cardName = "모든 적 방어력 감소";
                break;
            case Card.CardType.AttackSpeedUp:
                cardName = "아군 유닛 공격속도 증가";
                break;
        }
    }
}
