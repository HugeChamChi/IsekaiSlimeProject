using System;
using Units;
using UnityEngine;

namespace Util
{
    public class CameraRay : MonoBehaviour
    {
        private Camera _cam;
        private UnitHolder _holder;
        
        //RayCast 사용
        private void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                
                //스킬 범위 가리기
                _holder?.HideSkillRange();
                
                if (hit.collider != null && hit.transform.TryGetComponent<UnitHolder>(out UnitHolder holder))
                {
                    Debug.Log("클릭된 유닛 ");
                    _holder = holder;
                    //todo: 공격 범위, 스킬 범위 보여주기(shader), 시간 등
                    holder.ShowSkillRange();
                    
                }

            }
        }
        
        
        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }
        

    }
}