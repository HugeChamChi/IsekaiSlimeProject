using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "Card/Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea]
    public string description;
    public Sprite icon;
    public CardEffect effect;

#if UNITY_EDITOR
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

        description = $"{cardName} : +{effect.amount}";

        string newName = $"{effect.type}_{effect.amount}";
        string assetPath = AssetDatabase.GetAssetPath(this);
        string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        if (currentName != newName)
        {
            AssetDatabase.RenameAsset(assetPath, newName);
        }

        name = newName;
    }
#endif
}
