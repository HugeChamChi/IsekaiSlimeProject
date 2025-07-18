using Card;
using Managers;
using System.Collections;
using System.Collections.Generic;
using Units;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEffect", menuName = "Card/Effect")]
public class CardEffect : ScriptableObject
{
    public CardType type;
    public float amount;

    public void Execute()
    {
        switch (type)
        {
            case CardType.AttackPowerUp:
                Manager.Card.AttackPower.Value += amount/100;
                break;
            case CardType.GoldGainUp:
                Manager.Card.GoldGainUp.Value += amount/100;
                break;
            case CardType.AllEnemyDefenseDown:
                Manager.Card.AllEnemyDefenseDown.Value -= amount/100;
                break;
            case CardType.AttackSpeedUp:
                Manager.Card.AttackSpeed.Value += amount/100;
                break;
        }
    }
#if UNITY_EDITOR
    [ContextMenu("Random Value")]
    public void Random()
    {
        type = (CardType)UnityEngine.Random.Range(0, 4);
        amount = UnityEngine.Random.Range(0, 100);        
    }

    private void OnValidate()
    {
        string newName = $"{type}_{amount}";
        string assetPath = AssetDatabase.GetAssetPath(this);
        string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        if(currentName != newName)
        {
            AssetDatabase.RenameAsset(assetPath, newName);
            AssetDatabase.SaveAssets();
        }

        name = newName;
    }
#endif
}