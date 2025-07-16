using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// 게임 내 유닛 데이터들을 관리하는 싱글턴 매니저.
    /// JSON에서 유닛 정보를 로드하고,
    /// 인덱스 기반 조회 및 랜덤 유닛 선택 기능 제공.
    /// </summary>
    public class UnitDataManager : DesignPattern.Singleton<UnitDataManager>
    {
        /// <summary>
        /// 유닛 데이터 딕셔너리 (Key : Index → Value : UnitInfo)
        /// </summary>
        private Dictionary<int, UnitInfo> UnitInfoDict;


        #region Unity LifeCycle

        private void Awake() => SingletonInit();
        private void Start() => Init();


        #endregion
        
        
        /// <summary>
        /// 매니저 초기화: 딕셔너리 생성 및 데이터 로드.
        /// </summary>
        private void Init()
        {
            UnitInfoDict = new();
            LoadDataFromJson();
        }

        /// <summary>
        /// (임시)Resources 폴더에서 UnitData.json을 읽어와 유닛 데이터 로드.
        /// </summary>
        private void LoadDataFromJson()
        {
            //todo: 파이어베이스 연동 시 수정하기
            var jsonText = Manager.Resources.Load<TextAsset>("Data/UnitData").text;
            var unitCollection = JsonUtility.FromJson<UnitDataCollection>(jsonText);
            
            foreach (var data in unitCollection.units)      
            {
                UnitInfoDict[data.Index] = data;
            }
            
            Debug.Log($"UnitDataManager: {UnitInfoDict.Count}개의 유닛 데이터를 로드했습니다.");
        }
        
        
        /// <summary>
        /// 특정 인덱스의 유닛 데이터를 깊은 복사(클론)하여 반환.
        /// </summary>
        /// <param name="index">유닛 인덱스</param>
        /// <returns>클론된 UnitInfo</returns>
        public UnitInfo GetUnitData(int index)
        {
            if (UnitInfoDict.TryGetValue(index, out var data))
                return data.Clone();
            Debug.LogWarning($"[UnitDataManager] index {index}에 해당하는 유닛을 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 등록된 유닛들 중 무작위로 하나의 인덱스를 반환.
        /// </summary>
        /// <returns>랜덤 유닛 인덱스</returns>
        public int PickRandomUnitIndex()
        {
            //todo: 수정 필요
            var randomItem = UnitInfoDict.ElementAt(Random.Range(0, UnitInfoDict.Count));
            return randomItem.Key; // index만 반환
        }
        
        
        
        //todo : 데이터 테이블 확정시 수정 필요
        /// <summary>
        /// JSON 파싱용 유닛 데이터 컬렉션.
        /// </summary>
        [System.Serializable]
        private class UnitDataCollection
        {
            public List<UnitInfo> units;
        }
        
        /// <summary>
        /// 단일 유닛 정보 클래스.
        /// </summary>
        [System.Serializable]
        public class UnitInfo
        {
            public int Index;
            public string Name;
            public int Tier;
            public int Type;
            public string Description;
            public string ModelFileName;
            public float Attack;
            public float AttackSpeed;
            public float AttackRange;
            public float BuffRange;


            /// <summary>
            /// 복사 생성자: 다른 UnitInfo에서 값 복사.
            /// </summary>
            public UnitInfo(UnitInfo original)
            {
                Index = original.Index;
                Name = original.Name;
                Tier = original.Tier;
                Type = original.Type;
                Description = original.Description;
                ModelFileName = original.ModelFileName;
                Attack = original.Attack;
                AttackSpeed = original.AttackSpeed;
                AttackRange = original.AttackRange;
                BuffRange = original.BuffRange;
            }

            /// <summary>
            /// 현재 인스턴스의 깊은 복사본을 생성.
            /// </summary>
            public UnitInfo Clone()
            {
                return new UnitInfo(this);
            }
            
        }

    }
    
  
}