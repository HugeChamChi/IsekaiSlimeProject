using Managers;
using PlayerField;
using System;
using Units;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Util
{
    public class CameraRay : MonoBehaviour
    {
        private Camera _cam;
        private UnitHolder _moveHolder;
        
        //RayCast 사용
        private void Update()
        {

            //ui 클릭은 제외
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                MouseButtonDown();  
            }

            //드래그
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                MouseButton();   
            }


            if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                MouseButtonUp();
            }
        }

        private void MouseButtonDown()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
                
            //스킬 범위 가리기
            if (GameManager.Instance.SelectedHolder!=null)
            {
                GameManager.Instance.SelectedHolder.HideSkillRange();
                GameManager.Instance.ClearSelectedHolder();
            }
                
            if (hit.collider != null && hit.transform.TryGetComponent<UnitHolder>(out UnitHolder holder))
            {
                GameManager.Instance.SetSelectedHolder(holder);

            }
        }

     

        private void MouseButton()
        {
            if (GameManager.Instance.SelectedHolder != null)
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null && GameManager.Instance.SelectedHolder.transform != hit.transform)
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
                    if(GameManager.Instance.SelectedHolder?.transform == hit.collider.transform)
                        //todo: 공격 범위, 스킬 범위 보여주기(shader), 시간 등
                        GameManager.Instance.SelectedHolder?.ShowSkillRange();
                }
            }

            else
            {
                if (GameManager.Instance.SelectedHolder.CurrentUnit != null)
                {
                    PlayerFieldController.SwapHolderPosition(GameManager.Instance.SelectedHolder, _moveHolder);
                }
            }
            
            _moveHolder = null;

        }

        
        
        public void SetCamera(Camera cam)
        {
            _cam = cam;
        }
        
   

    }
}