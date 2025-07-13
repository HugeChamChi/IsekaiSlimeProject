using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

namespace Util
{
    public static class Utils
    {
        private static readonly Dictionary<float, WaitForSeconds> delayDic = new Dictionary<float, WaitForSeconds>();
        private static readonly StringBuilder sb = new StringBuilder();
        private static Collider2D[] buffer = new Collider2D[100];

        /// <summary>
        /// �ڷ�ƾ ������ �������� �Լ�
        /// </summary>       
        public static WaitForSeconds GetDelay(float delay)
        {
            if (delayDic.ContainsKey(delay))
                return delayDic[delay];

            delayDic[delay] = new WaitForSeconds(delay);
            return delayDic[delay];
        }

        /// <summary>
        /// StringBuilder�� Text�� �߰��ϴ� �Լ� (�� �ٲ� X)
        /// </summary>        
        public static void Append(string text) => sb.Append(text);

        /// <summary>
        /// StringBuilder�� Text�� �߰��ϴ� �Լ� (�� �ٲ� O)
        /// </summary>        
        public static void AppendLind(string text) => sb.AppendLine(text);

        /// <summary>
        /// StringBuilder�� ����� String�� �������� StringBuilder�� �ʱ�ȭ�ϴ� �Լ�
        /// </summary>        
        public static string GetText()
        {
            string temp = sb.ToString();
            sb.Clear();
            return temp;
        }

        /// <summary>
        /// Ÿ������ ���ϴ� ������ ���ϴ� �Լ�
        /// </summary>
        public static Vector2 DirToTarget(Vector2 target, Vector2 onwer)
        {
            return (target - onwer).normalized;
        }


        /// <summary>
        /// ����� ���� ã�µ� ���� ���� ����
        /// </summary>
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
