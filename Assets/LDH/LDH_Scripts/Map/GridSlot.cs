using Photon.Pun;
using UnityEngine;

namespace PlayerField
{
    public class GridSlot : NetworkUnit
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public Vector2 SpawnPosition { get; private set; }
        public bool IsOccupied { get; set; }
        public SlotType Type { get; private set; }
        
        public Color originColor = Color.white;
        public Color skillColor = Color.yellow;

        public SpriteRenderer sr;

        public void SetupGridSlot(int row, int column, Vector2 spawnPosition, SlotType type, Vector3 slotSize, Transform parent, bool isOccupied = false)
        {
   
            Row = row;
            Column = column;
            SpawnPosition = spawnPosition;
            Type = type;
            IsOccupied = isOccupied;
            transform.localScale = slotSize;
            
            
            
            // photonView.RPC("SetParentAndScale", RpcTarget.AllBuffered, parent.GetComponent<PhotonView>().ViewID, slotSize);

            // transform.localScale = slotSize;
            // transform.SetParent(parent);
            // gameObject.name = $"Grid {row}_{column}";
        }
        
        
        [PunRPC]
        public void SetParentAndScale(int parentViewID, Vector3 scale)
        {
            var parentTransform = PhotonView.Find(parentViewID).transform;
            transform.localScale = scale;
            transform.SetParent(parentTransform);
        }
        
        //임시
        public void SetColor(bool isSkill)
        {
            sr ??= GetComponent<SpriteRenderer>();
            sr.color = isSkill ? skillColor : originColor;
        }
    }

}