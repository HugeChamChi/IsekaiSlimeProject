using Managers;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayerField
{
    public class GridSlot : NetworkUnit
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public Vector2 SpawnPosition { get; private set; }
        public bool IsOccupied { get; set; }
        public SlotType SlotType { get; private set; }

        private Color innerColor = Color.white;
        private Color outerColor = Color.gray;

        public Color originColor => SlotType == SlotType.Inner ? innerColor : outerColor;
        public Color skillColor = Color.yellow;

        public SpriteRenderer sr;

        public void SetupGridSlot(int row, int column, Vector2 spawnPosition, SlotType type, Vector3 slotSize, int parentUniqueID, bool isOccupied = false)
        {
   
            Row = row;
            Column = column;
            SpawnPosition = spawnPosition;
            SlotType = type;
            IsOccupied = isOccupied;
            transform.localScale = slotSize;
            
            SetParent(parentUniqueID);
            //ComponentProvider.Get<PhotonView>(gameObject).RPC("SetParentRPC", RpcTarget.All, parentUniqueID);
            
            
            // photonView.RPC("SetParentAndScale", RpcTarget.AllBuffered, parent.GetComponent<PhotonView>().ViewID, slotSize);

            // transform.localScale = slotSize;
            // transform.SetParent(parent);
            // gameObject.name = $"Grid {row}_{column}";
        }
        
        public void SetParent(int parentUniqueID)
        {
            Transform parent = GameManager.Instance.GetInGameObjectByID(parentUniqueID);
            Transform child = transform;
            
            if (child == null || parent == null)
            {
                Debug.LogWarning("Parent failed: null reference");
                return;
            }
            child.SetParent(parent);
        }
        
        
        //임시
        public void SetColor(bool isSkill)
        {
            sr ??= GetComponent<SpriteRenderer>();
            sr.color = isSkill ? skillColor : originColor;
        }
        
        
    }

}