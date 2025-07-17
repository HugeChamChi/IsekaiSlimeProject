using Managers;
using Photon.Pun;
using Unit;
using UnityEngine;
using Util;

namespace Units
{
    /// <summary>
    /// 게임 내 유닛(영웅) 기본 클래스.
    /// 유닛 기본 정보 보유
    /// </summary>
    public class Unit : NetworkUnit, IPunInstantiateMagicCallback
    {
        
        #region Variables

        [field: Header("Unit Info")]
        [field: SerializeField] public int Index { get; private set; }
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public UnitTier Tier { get; private set; }
        [field: SerializeField] public UnitType Type { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField] public string ModelFileName { get; private set; }
        [field: SerializeField] public SkillRangeType SkillRangeType { get; set; } //todo :  setter private으로 바꾸기
        /// <summary>
        /// 유닛의 컨트롤러 컴포넌트 (Stat, Animator, Attack, Skill 관리)
        /// </summary>
        public UnitController Controller;
        
        
        #endregion
        
        
        #region Unity LifeCycle

        //private void Awake() => Init();

        //private void Start() { }

        #endregion
        
  

        #region Unit Initialization
        
        
        /// <summary>
        /// 유닛 데이터로 초기화.
        /// Controller에도 데이터 전달.
        /// </summary>
        /// <param name="info">유닛 정보</param>
        protected virtual void InitData(UnitDataManager.UnitInfo info)
        {
            Index = info.Index;
            Name = info.Name;
            Tier = (UnitTier)info.Tier;
            Type = (UnitType)info.Type;
            Description = info.Description;
            ModelFileName = info.ModelFileName;
            SkillRangeType = (SkillRangeType)info.SkillRangeType;
            
            Controller.InitData(info);
            
            Debug.Log($"[Unit] 초기화 완료! 이름: {Name}");
            
        }
        #endregion

        #region Unit Delete

        /// <summary>
        /// 네트워크 상에서 유닛 제거.
        /// </summary>
        public void UnitDelete()
        {
            Manager.Resources.NetworkDestroy(gameObject);
            // TODO: PlayerFieldManager의 슬롯 상태 업데이트 필요
        }
        

        #endregion
        
        #region Photon Callback
        /// <summary>
        /// Photon Instantiate 시 InstantiationData로 전달된 index 기반 데이터 로드.
        /// </summary>
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;
            if (instantiationData == null || instantiationData.Length == 0)
            {
                Debug.LogError("[Unit] InstantiationData 비어있음!");
                return;
            }
            
            int unitIndex = (int)instantiationData[0];
            var unitInfo = Manager.UnitData.GetUnitData(unitIndex);
            
            InitData(unitInfo);
        }
        #endregion
        
        #region Legacy(미사용)
        private void SetPositionScale()
        {
            // PlayerFieldController field = PlayerFieldManager.Instance.GetFieldController(_ownerActorNumber);
            // if (field == null)
            // {
            //     Debug.LogError($"{_ownerActorNumber}의 필드 위치를 찾을 수 없습니다.");
            //     return;
            // }
            //
            // //position 설정
            // transform.position = field.SpawnList[_slotIndex];
            //
            // //scale 설정
            // transform.localScale = Vector3.one * field.transform.localScale.x;
        }
        

        #endregion


    }
}
