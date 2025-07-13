using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class UnitSpawner : MonoBehaviour
    {
        [Header("Spawn Setting")] 
        [SerializeField] private Transform _spawnPanel; // 유닛이 배치될 부모 패널
        [SerializeField] private int _xCount = 4; // X축 슬롯 개수 (열 개수)
        [SerializeField] private int _yCount = 6; // Y축 슬롯 개수 (행 개수)

        [Header("Unit")] 
        [SerializeField] private GameObject _unitPrefab;
        
        
        [Header("UI")] 
        [SerializeField] private Button _summonButton;
        
        
        
        
        //데이터만 들고 있으면 되니까 list로 좌표 저장
        private List<Vector2> spawnList = new();



        #region Unity LifeCycle Function

        private void Awake() => Init();
        private void Start() =>GenerateGridSlots();
        private void OnDestroy() => UnSubscribe();
        
        #endregion
        
        
        //참조 초기화
        private void Init()
        {
            Subscribe();
        }
        
        //이벤트
        private void Subscribe()
        {
            _summonButton.onClick.AddListener(Summon);
        }

        private void UnSubscribe()
        {
            _summonButton.onClick.RemoveListener(Summon);
        }
        
        
        
        #region Make Grid
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
            
            //슬롯 시작 포지션 좌표 계산
            float startX = panelWidth / 2 - slotScaleX / 2;
            float startY = panelHeight / 2 - slotScaleY / 2;
            
            
            // 슬롯들을 묶을 부모 오브젝트 생성

            
            // grid 배열의 spawn slot 생성
            for (int row = 0; row < _yCount; row++)
            {
                for (int column = 0; column < _xCount; column++)
                {
                    
                    //position 계산
                    float xPos = startX - column * slotScaleX;
                    float yPos = startY - row * slotScaleY;
                    Vector3 slotPos = new Vector2(xPos, yPos) + (Vector2)_spawnPanel.position;
                    
                    spawnList.Add(slotPos);
                    

                }
            }
        }
        #endregion

        #region Summon

        private void Summon()
        {
            
        }

        #endregion

    }
}


