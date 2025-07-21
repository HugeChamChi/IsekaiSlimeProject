using Managers;
using System;
using Units;
using UnityEngine;
using UnityEngine.UI;

namespace LDH.LDH_Scripts.Temp
{
    public class DeleteUnitButton : MonoBehaviour
    {
        public Button button;
        private void Start()
        {
            
            button.interactable = InGameManager.Instance.SelectedHolder != null;
            InGameManager.Instance.OnSelectedHolderChanged += OnSelectedHolderChanged;
            button.onClick.AddListener(() =>
            {
                if (InGameManager.Instance.SelectedHolder != null)
                {
                    InGameManager.Instance.SelectedHolder.DeleteUnit();
                    InGameManager.Instance.ClearSelectedHolder();
                }
            });
        }
        
        
        private void OnSelectedHolderChanged(UnitHolder holder)
        {
            button.interactable = (holder != null && holder.CurrentUnit!=null);
        }

    }
}