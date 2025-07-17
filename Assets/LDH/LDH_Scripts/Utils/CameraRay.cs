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

                if (hit.collider != null && hit.transform.TryGetComponent<UnitHolder>(out UnitHolder holder))
                {
                    _holder = holder;
                    //todo: 공격 범위, 스킬 범위
                    holder.ShowSkillRange();
                    
                    //todo: shader
                    
                }

            }
        }
        
        
        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }
        

    }
}