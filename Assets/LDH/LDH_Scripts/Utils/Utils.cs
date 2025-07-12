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
        /// �ӽ� �Լ� ���� ����
        /// </summary>        
        public static int FindTarget(Transform transform, Collider2D[] cols, float distance, LayerMask targetLayer)
        {
            return Physics2D.OverlapCircleNonAlloc(transform.position, distance, cols, targetLayer);
        }
    }
}
