using DesignPattern;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        private int idCounter = 1000; // 시작 값 (원하면 1, 1000, 10000 등으로)

        public Dictionary<int, Transform> inGameObjects = new();

        public int GenerateUniqueID()
        {
            idCounter++;
            return idCounter;
        }
        
        public void RegisterInGameObject(InGameObject obj)
        {
            inGameObjects[obj.uniqueID] = obj.transform;
            Debug.Log($"Registered object {obj.name} with ID {obj.uniqueID}");
        }
        public Transform GetInGameObjectByID(int uniqueID)
        {
            inGameObjects.TryGetValue(uniqueID, out var transform);
            return transform;
        }
    }
}