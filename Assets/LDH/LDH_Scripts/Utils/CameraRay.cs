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
            
            if (hit.collider != null && hit.transform.TryGetComponent<UnitHolder>(out UnitHolder holder))
            {
                if (holder != InGameManager.Instance.SelectedHolder)
                {
                    InGameManager.Instance.SelectedHolder?.HideSkillRange();
                
                    InGameManager.Instance.ClearSelectedHolder();
                    Manager.UI.characterInfoPanel.Close();
                    
                }
                InGameManager.Instance.SetSelectedHolder(holder);
                

            }
            else
            {
                InGameManager.Instance.SelectedHolder?.HideSkillRange();
                InGameManager.Instance.ClearSelectedHolder();
                Manager.UI.characterInfoPanel.Close();
            }
        }

     

        private void MouseButton()
        {
            if (InGameManager.Instance.SelectedHolder != null)
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null && InGameManager.Instance.SelectedHolder.transform != hit.transform)
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
                    if (InGameManager.Instance.SelectedHolder?.transform == hit.collider.transform)
                    {
                        if (InGameManager.Instance.SelectedHolder != null &&
                            InGameManager.Instance.SelectedHolder.CurrentUnit != null
                           )
                        {
                            //todo: 공격 범위, 스킬 범위 보여주기(shader), 시간 등
                            InGameManager.Instance.SelectedHolder?.ShowSkillRange();
                            Manager.UI.characterInfoPanel.Show(InGameManager.Instance.SelectedHolder.CurrentUnit);
                        }
                    }
                }
            }

            else
            {
                if (InGameManager.Instance.SelectedHolder.CurrentUnit != null)
                {
                    PlayerFieldController.SwapHolderPosition(InGameManager.Instance.SelectedHolder, _moveHolder);
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