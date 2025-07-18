using Managers;
using PlayerField;
using System;
using Units;
using Unity.VisualScripting;
using UnityEngine;

namespace Util
{
    public class CameraRay : MonoBehaviour
    {
        private Camera _cam;
        private UnitHolder _holder;
        private UnitHolder _moveHolder;
        
        //RayCast 사용
        private void Update()
        {

            if (Input.GetMouseButtonDown(0))
            {
                MouseButtonDown();  
            }

            //드래그
            if (Input.GetMouseButton(0))
            {
                MouseButton();   
            }


            if (Input.GetMouseButtonUp(0))
            {
                MouseButtonUp();
            }
        }

        private void MouseButtonDown()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                
            //스킬 범위 가리기
            if (_holder!=null)
            {
                _holder.HideSkillRange();
                _holder = null;
            }
                
            if (hit.collider != null && hit.transform.TryGetComponent<UnitHolder>(out UnitHolder holder))
            {
                _holder = holder;
            }
        }

     

        private void MouseButton()
        {
            if (_holder != null)
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null && _holder.transform != hit.transform)
                {
                    _moveHolder = hit.collider.GetComponent<UnitHolder>();
                }
            }
        }
        
        
             
        private void MouseButtonUp()
        {

            if (_moveHolder == null)
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    if(_holder.transform == hit.collider.transform)
                        //todo: 공격 범위, 스킬 범위 보여주기(shader), 시간 등
                        _holder?.ShowSkillRange();
                }
            }

            else
            {
                PlayerFieldController.SwapHolderPosition(_holder, _moveHolder);
            }
            
            _moveHolder = null;

        }

        
        
        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }
        
   

    }
}