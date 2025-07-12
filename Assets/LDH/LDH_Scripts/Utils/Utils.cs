using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Util
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

        public static Transform FindClosestTarget(Vector2 origin, float range, OverlapType type, LayerMask layerMask, float boxAngle = 0f, Vector2? boxSize = null,
        Func<GameObject, bool> filter = null)
        {
            int count = GetTargets(origin, range, type, layerMask, boxAngle, boxSize, buffer);
 
            Transform best = null;
            float bestValue = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Transform target = buffer[i].transform;

                if (!filter(target.gameObject))
                    continue;

                float value = Vector2.Distance(origin, target.position);

                if (value < bestValue)
                {
                    bestValue = value;
                    best = target;
                }
            }

            return best;
        }

        public static int GetTargets(Vector2 origin, float range, OverlapType type, LayerMask layerMask, float boxAngle, Vector2? boxSize, Collider2D[] results)
        {            
            switch (type)
            {
                case OverlapType.Circle:
                    return Physics2D.OverlapCircleNonAlloc(origin, range, results, layerMask);

                case OverlapType.Box:
                    Vector2 size = boxSize ?? new Vector2(range, range);
                    return Physics2D.OverlapBoxNonAlloc(origin, size, boxAngle, results, layerMask);

                default:
                    return -1;
            }
        }
    }
}
