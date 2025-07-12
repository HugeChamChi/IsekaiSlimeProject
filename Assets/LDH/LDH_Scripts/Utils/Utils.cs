using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace Utile
{
    public static class Utils
    {
        private static readonly Dictionary<float, WaitForSeconds> delayDic = new Dictionary<float, WaitForSeconds>();
        private static readonly StringBuilder sb = new StringBuilder();
        private static Collider2D[] buffer = new Collider2D[100];

        /// <summary>
        /// 코루틴 딜레이 가져오는 함수
        /// </summary>       
        public static WaitForSeconds GetDelay(float delay)
        {
            if (delayDic.ContainsKey(delay))
                return delayDic[delay];

            delayDic[delay] = new WaitForSeconds(delay);
            return delayDic[delay];
        }

        /// <summary>
        /// StringBuilder에 Text를 추가하는 함수 (줄 바꿈 X)
        /// </summary>        
        public static void Append(string text) => sb.Append(text);

        /// <summary>
        /// StringBuilder에 Text를 추가하는 함수 (줄 바꿈 O)
        /// </summary>        
        public static void AppendLind(string text) => sb.AppendLine(text);

        /// <summary>
        /// StringBuilder에 저장된 String을 가져오고 StringBuilder를 초기화하는 함수
        /// </summary>        
        public static string GetText()
        {
            string temp = sb.ToString();
            sb.Clear();
            return temp;
        }

        /// <summary>
        /// 타겟으로 향하는 방향을 구하는 함수
        /// </summary>
        public static Vector2 DirToTarget(Vector2 target, Vector2 onwer)
        {
            return (target - onwer).normalized;
        }

        /// <summary>
        /// 임시 함수 수정 예정
        /// </summary>        
        public static int FindTarget(Transform transform, Collider2D[] cols, float distance, LayerMask targetLayer)
        {
            return Physics2D.OverlapCircleNonAlloc(transform.position, distance, cols, targetLayer);
        }
    }
}
