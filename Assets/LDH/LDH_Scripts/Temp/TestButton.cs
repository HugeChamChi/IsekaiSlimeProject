using Managers;
using System;
using Units;
using UnityEngine;
using UnityEngine.UI;

namespace LDH.LDH_Scripts.Temp
{
    public class TestButton : MonoBehaviour
    {
        public Button button;
        private void Awake()
        {
            
            button.interactable = GameManager.Instance.SelectedHolder != null;
            GameManager.Instance.OnSelectedHolderChanged += OnSelectedHolderChanged;
            button.onClick.AddListener(() =>
            {
                if (GameManager.Instance.SelectedHolder != null)
                {
                    GameManager.Instance.SelectedHolder.Sell();
                    GameManager.Instance.ClearSelectedHolder();
                }
            });
        }
        
        
        private void OnSelectedHolderChanged(UnitHolder holder)
        {
            button.interactable = holder != null;
        }

    }
}