using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabCreater : MonoBehaviour
{
    [Serializable]
    public struct MonsterPrefabData
    {
        public RuntimeAnimatorController runtimeAnimatorControllers;
        public MonsterStat monsterStats;
        public string monsterName;
        public int ID;
    }    

    [SerializeField] private string path;
    [SerializeField] private MonsterPrefabData[] monsterPrefabDatas;

    [ContextMenu("Create_Prefab")]
    public void CreatePrefabs()
    {
        for (int i = 0; i < monsterPrefabDatas.Length; i++)
        {
            GameObject obj = new GameObject(monsterPrefabDatas[i].monsterName);

            Animator animator = obj.AddComponent<Animator>();
            
            RuntimeAnimatorController controller = monsterPrefabDatas[i].runtimeAnimatorControllers;

            if(controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            BaseMonster monster = obj.AddComponent<BaseMonster>();
            MonsterStatusController monsterStatusController = obj.AddComponent<MonsterStatusController>();
            obj.AddComponent<BoxCollider2D>();
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Monster";

            monsterPrefabDatas[i].monsterStats.ID = monsterPrefabDatas[i].ID;
            monsterStatusController.baseStat = monsterPrefabDatas[i].monsterStats;

            string savePath = $"{path}/{monsterPrefabDatas[i].monsterName}.prefab";

            PrefabUtility.SaveAsPrefabAsset(obj, savePath);

            GameObject.DestroyImmediate(obj);
        }
    }
}
