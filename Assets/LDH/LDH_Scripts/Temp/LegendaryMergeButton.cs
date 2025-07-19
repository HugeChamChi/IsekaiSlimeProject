using Managers;
using PlayerField;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LegendaryMergeButton : MonoBehaviour
{
    private Button _button;

    private void Start() => Init();

    private void Init()
    {
        _button = GetComponent<Button>();
        
        PlayerFieldManager.Instance.OnRegisterLocalFieldcontroller += SetupButtonEvent;
    }


    private void SetupButtonEvent(PlayerFieldController fieldController)
    {
        _button.onClick.AddListener(PlayerFieldManager.Instance.UnitSpanwer.MergeEpicUnits);
        
        fieldController.OnEpicUnitCountChanged += SetButtonInteractable;
    }

    private void SetButtonInteractable(int currentEpicCount)
    {
        _button.interactable = currentEpicCount >= 5;
    }
    

}
