using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Spawn Setting")] 
        [SerializeField] private Transform _spawnPanel; // 유닛이 배치될 부모 패널
        [SerializeField] private int _xCount = 4; // X축 슬롯 개수 (열 개수)
        [SerializeField] private int _yCount = 6; // Y축 슬롯 개수 (행 개수)

        
        private void Start() => GenerateGridSlots();

        
        /// <summary>
        /// 유닛이 배치되는 패널 영역에 그리드 형태로 슬롯을 생성하고 배치한다.
        /// 우측 상단부터 시작해서 좌로 이동, 아래로 이동하며 순서대로 배치
        /// </summary>
        private void GenerateGridSlots()
        {
            // 부모의 로컬 사이즈 계산
            Bounds panelBounds = _spawnPanel.GetComponent<SpriteRenderer>().bounds;
            float panelWidth = panelBounds.size.x;
            float panelHeight = panelBounds.size.y;
            
            //슬롯 사이즈 계산
            float slotScaleX = _spawnPanel.localScale.x / _xCount;
            float slotScaleY = _spawnPanel.localScale.y / _yCount;
            // float slotScaleX = _spawnPanel.localScale.x / _xCount / _spawnPanel.lossyScale.x;
            // float slotScaleY = _spawnPanel.localScale.y / _yCount / _spawnPanel.lossyScale.y;
            Vector3 slotScale = new Vector3(slotScaleX, slotScaleY, 1);
            
            //슬롯 시작 포지션 좌표 계산
            float startX = panelWidth / 2 - slotScaleX / 2;
            float startY = panelHeight / 2 - slotScaleY / 2;
            
            
            // 슬롯들을 묶을 부모 오브젝트 생성

            
            // grid 배열의 spawn slot 생성
            for (int row = 0; row < _yCount; row++)
            {
                for (int column = 0; column < _xCount; column++)
                {
                    var slot = new GameObject($"Slot({column}, {row})");
                    
                    //todo: 임시로 색깔 추가
                    SpriteRenderer slotSp = slot.AddComponent<SpriteRenderer>();
                    slotSp.sprite = _spawnPanel.GetComponent<SpriteRenderer>().sprite;
                    slotSp.color = Color.blue;
                    
                    
                    //position 설정
                    float xPos = startX - column * slotScaleX;
                    float yPos = startY - row * slotScaleY;
                    slot.transform.position = new Vector3(xPos, yPos, 1) + _spawnPanel.position;
                    
                    //scale 설정
                    slot.transform.localScale = slotScale;
                    
                    slot.transform.SetParent(_spawnPanel, true); // gridParent에 붙이기





                }
            }
        }

    }
}


