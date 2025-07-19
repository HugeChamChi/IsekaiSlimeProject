using Managers;
using PlayerField;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LegendaryMergeButton : MonoBehaviour
{
    public int legendaruOrder = 0; //0번째, 1번째 중 하나
    private Button _button;

    private void Start() => Init();

    private void Init()
    {
        _button = GetComponent<Button>();
        
        PlayerFieldManager.Instance.OnRegisterLocalFieldcontroller += SetupButtonEvent;
    }


    private void SetupButtonEvent(PlayerFieldController fieldController)
    {
        _button.onClick.AddListener(()=>PlayerFieldManager.Instance.UnitSpanwer.MergeEpicUnits(legendaruOrder));
        
        fieldController.CanSpawnLegendary[legendaruOrder] += SetButtonInteractable;
    }

    private void SetButtonInteractable(bool canMerge)
    {
        _button.interactable = canMerge;
    }
    

}
