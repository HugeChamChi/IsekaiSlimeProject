using DesignPattern;
using System;
using System.Collections.Generic;
using Units;
using UnityEngine;

namespace Managers
{
    public class InGameManager : Singleton<InGameManager>
    {
        private int idCounter = 1000; // 시작 값 (원하면 1, 1000, 10000 등으로)
        public UnitHolder SelectedHolder { get; private set; }

        public event Action<UnitHolder> OnSelectedHolderChanged;
        
        
        public Dictionary<int, Transform> inGameObjects = new();

        public int GenerateUniqueID()
        {
            idCounter++;
            return idCounter;
        }
        
        public void RegisterInGameObject(InGameObject obj)
        {
            inGameObjects[obj.uniqueID] = obj.transform;
            //Debug.Log($"Registered object {obj.name} with ID {obj.uniqueID}");
        }
        public Transform GetInGameObjectByID(int uniqueID)
        {
            inGameObjects.TryGetValue(uniqueID, out var transform);
            return transform;
        }
        
        
        public void SetSelectedHolder(UnitHolder holder)
        {
            SelectedHolder = holder;
            OnSelectedHolderChanged?.Invoke(holder);

            Debug.Log($"[GameManager] Selected holder: {holder.name}");
        }
        public void ClearSelectedHolder()
        {
            SelectedHolder = null;
            OnSelectedHolderChanged?.Invoke(null);
            Debug.Log($"[GameManager] Cleared selected holder");
        }
        
        
    }
}