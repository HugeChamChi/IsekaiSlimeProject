using Managers;
using Photon.Pun;
using System;
using UnityEngine;


    public class InGameObject : MonoBehaviour
    {
        public int uniqueID;

        protected virtual void Awake()
        {
            ComponentProvider.Add<InGameObject>(gameObject);
            
            Init();
        }

        protected virtual void OnDestroy()
        {
            ComponentProvider.Remove<InGameObject>(gameObject);
        }
        
        private void Init()
        {
            //자동으로 유니크 아이디 생성해서 gamemanager에 등록하기
            uniqueID = InGameManager.Instance.GenerateUniqueID();
            InGameManager.Instance.RegisterInGameObject(this);
        }
        
        [PunRPC]
        public void SetParentRPC(int parentUniqueID)
        {
            Transform parent = InGameManager.Instance.GetInGameObjectByID(parentUniqueID);
            Transform child = transform;
            
            if (child == null || parent == null)
            {
                Debug.LogWarning("Parent failed: null reference");
                return;
            }
            child.SetParent(parent);
        }


    }
