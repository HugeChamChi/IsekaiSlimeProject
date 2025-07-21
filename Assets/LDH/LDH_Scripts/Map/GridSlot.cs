using Managers;
using Photon.Pun;
using Units;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayerField
{
    public class GridSlot : NetworkUnit
    {
        public int Row { get; private set; }
        public int Column { get; private set; }
        public Vector2 SpawnPosition { get; private set; }
        
        public UnitHolder Holder { get; set; }
        public bool IsOccupied => Holder.CurrentUnit != null;
        public SlotType SlotType { get; private set; }

        private Color innerColor = Color.white;
        private Color outerColor = Color.gray;

        public Color originColor => SlotType == SlotType.Inner ? innerColor : outerColor;
        public Color skillColor = Color.yellow;
        public Color selectedColor = Color.green;
        
        private bool isSelectedRangeOn = false; // 선택 표시용
        private bool isSkillRangeOn = false;    // 스킬 표시용
        
        public SpriteRenderer sr;

        public void SetupGridSlot(int row, int column, Vector2 spawnPosition, SlotType type, UnitHolder holder, Vector3 slotSize, int parentUniqueID, bool isOccupied = false)
        {
   
            Row = row;
            Column = column;
            SpawnPosition = spawnPosition;
            SlotType = type;
            Holder = holder;
            transform.localScale = slotSize;
            
            SetParent(parentUniqueID);
            SetColor(type);
        }

        public void SetHolder(UnitHolder holder)
        {
            Holder = holder;
        }
        
        public void SetParent(int parentUniqueID)
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
        
        
        //임시
        private void SetColor(bool isOn)
        {
            sr ??= GetComponent<SpriteRenderer>();

            if (!isOn)
            {
                if (isSkillRangeOn || isSelectedRangeOn) sr.color = isSkillRangeOn ? skillColor : selectedColor;
                else sr.color = originColor;
            }
            else
            {
                sr.color = isSkillRangeOn ? skillColor : selectedColor;
            }
            
            //sr.color = isSkillOn ? skillColor : originColor;
        }

        public void SetColor(SlotType slotType)
        {
            sr ??= GetComponent<SpriteRenderer>();
            sr.color = slotType == SlotType.Inner ? innerColor : outerColor;
        }


        public void SetSelectedRange(bool isOn)
        {
            isSelectedRangeOn = isOn;
            SetColor(isOn);
        }
        public void SetSkillRange(bool isOn)
        {
            isSkillRangeOn = isOn;
            SetColor(isOn);
        }
        
        
        
    }

}