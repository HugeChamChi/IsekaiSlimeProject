using System.Collections.Generic;
using System.Linq;
using Unit;
using Units;
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

        private Dictionary<UnitTier, List<int>> UnitIndexListByTier;
  

        #region Unity LifeCycle

        private void Awake() { }
        private void Start() => Init();


        #endregion
        
        
        /// <summary>
        /// 매니저 초기화: 딕셔너리 생성 및 데이터 로드.
        /// </summary>
        private void Init()
        {
            UnitInfoDict = new();
            UnitIndexListByTier = new();
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
                if (!UnitIndexListByTier.ContainsKey((UnitTier)data.Tier))
                    UnitIndexListByTier[(UnitTier)data.Tier] = new List<int>();
                
               UnitIndexListByTier[(UnitTier)data.Tier].Add(data.Index);

                
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
        
        
        public List<int> GetUnitIndicesByTier(UnitTier tier)
        {
            if (UnitIndexListByTier.TryGetValue(tier, out var list))
                return new List<int>(list); // 복사본 반환
            return new List<int>();
        }
        
        public int PickRandomUnitIndexByTier(UnitTier tier)
        {
            if (UnitIndexListByTier.TryGetValue(tier, out var list) && list.Count > 0)
            {
                int randomIndex = Random.Range(0, list.Count);
                return list[randomIndex];
            }
            Debug.LogWarning($"[UnitDataManager] Tier {tier}에 해당하는 유닛이 없습니다.");
            return -1; // 실패 시 -1 같은 실패 코드 리턴
        }


        public int GetLegendaryIndex(int order) //0부터 시작
        {
            var legendaryList = GetUnitIndicesByTier(UnitTier.Legendary);
            if (legendaryList.Count < order || order < 0) return -1;
            return legendaryList[order];
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
            public int UnitType;
            public string Description;
            public string SkillDescription;
            public string ModelFileName;
            public float Attack;
            public float AttackSpeed;
            public int SkillDamage;
            public float SkillDuration;
            public float EffectDuration;
            public float SkillCoolTime;
            public float AttackRange;
            public int SkillRangeType;
            public float EffectAmount;
            

            /// <summary>
            /// 복사 생성자: 다른 UnitInfo에서 값 복사.
            /// </summary>
            public UnitInfo(UnitInfo original)
            {
                Index = original.Index;
                Name = original.Name;
                Tier = original.Tier;
                UnitType = original.UnitType;
                Description = original.Description;
                ModelFileName = original.ModelFileName;
                Attack = original.Attack;
                AttackSpeed = original.AttackSpeed;
                AttackRange = original.AttackRange;
                SkillRangeType = original.SkillRangeType;

                SkillDescription = original.SkillDescription;
                SkillDamage = original.SkillDamage;
                SkillDuration = original.SkillDuration;
                EffectDuration = original.EffectDuration;
                SkillCoolTime = original.SkillCoolTime;

                EffectAmount = original.EffectAmount;

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