using Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardPanel : MonoBehaviour
{
    [SerializeField] TMP_Text goldAmount;
    [SerializeField] TMP_Text gemAmount;

    private void OnEnable()
    {
        UpdateGoldText(Manager.Data.Gold.Value);
        UpdateGemText(Manager.Data.Gem.Value);
        Manager.Data.Gold.AddEvent(UpdateGoldText);
        Manager.Data.Gem.AddEvent(UpdateGemText);
    }

    private void OnDisable()
    {
        Manager.Data.Gold.RemoveEvent(UpdateGoldText);
        Manager.Data.Gem.RemoveEvent(UpdateGemText);
    }

    private void UpdateGoldText(int amount)
    { 
        goldAmount.text = amount.ToString();
    }

    private void UpdateGemText(int amount)
    {
        gemAmount.text = amount.ToString();
    }
}
