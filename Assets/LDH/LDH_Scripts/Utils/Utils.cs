using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

                if (filter!=null && !filter(target.gameObject))
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

        
        //페이드 효과
        #region Fade Effect
        
        /// <summary>
        /// 공통 Fade 처리 메서드.
        /// 대상의 color getter/setter를 받아 alpha 값을 보간하며 페이드 효과를 적용.
        /// </summary>
        /// <param name="getColor">현재 색상을 가져오는 함수</param>
        /// <param name="setColor">색상 적용하는 함수</param>
        /// <param name="start">사작 alpha 값(0~1)</param>
        /// <param name="end">종료 alpha (0~1)</param>
        /// <param name="fadeTime">페이드에 걸리는 시간 (초)</param>
        /// <param name="delay">시작 전 대기 시간 (초)</param>
        /// <param name="action">완료 시 실행할 콜백</param>
        public static IEnumerator Fade(
            System.Func<Color> getColor,
            System.Action<Color> setColor,
            float start, float end,
            float fadeTime = 1f,
            float delay = 0f,
            Action action = null
            )
        {
            if (delay > 0) yield return new WaitForSeconds(delay);
            
            // getColor나 setColor가 null이면 실행할 대상이 없으므로 중단
            if(getColor == null || setColor == null) yield break;

            float percent = 0f;
            float elapsedTime = 0f;

            while (percent < 1)
            {
                //UI 페이드에도 사용하도록 unsacledDeltaTime 사용(타임스케일 영향을 받지 않는 실제 프레임 경과 시간)
                elapsedTime += Time.unscaledDeltaTime;
                
                percent = Mathf.Clamp01(elapsedTime / fadeTime); // percent 값이 0~1 사이를 넘지 않게 처리
                
                //색상 설정
                Color color = getColor();
                color.a = Mathf.Lerp(start, end, percent);
                setColor(color);
                
                yield return null;
            }
            action?.Invoke();
        }
        
        
        //=====각 컴포넌트 별 Wrapper 메서드=====//
        
        /// <summary>
        /// SpriteRenderer용 페이드 효과.
        /// </summary>
        public static IEnumerator Fade(SpriteRenderer target, float start, float end, float fadeTime = 1, float delay = 0, Action action = null)
        {
            if (target == null) yield break;
            yield return Fade(() => target.color,
                c => target.color = c, 
                start, end, fadeTime, delay, action);
        }
        
        /// <summary>
        /// Image용 페이드 효과.
        /// 비활성화된 GameObject는 먼저 활성화시킴.
        /// </summary>
        public static IEnumerator Fade(Image target, float start, float end, float fadeTime = 1, float delay = 0, Action action = null)
        {
            if (target == null) yield break;
            
            if (!target.gameObject.activeSelf)
                target.gameObject.SetActive(true);

            yield return Fade(() => target.color, 
                c => target.color = c, 
                start, end, fadeTime, delay, action);
        }
        
        /// <summary>
        /// Tilemap용 페이드 효과.
        /// </summary>
        public static IEnumerator Fade(Tilemap target, float start, float end, float fadeTime = 1, float delay = 0, Action action = null)
        {
            if (target == null) yield break;
            yield return Fade(() => target.color, 
                c => target.color = c, 
                start, end, fadeTime, delay, action);
        }
        
      
        /// <summary>
        /// TextMeshProUGUI용 페이드 효과.
        /// </summary>
        public static IEnumerator Fade(TextMeshProUGUI target, float start, float end, float fadeTime = 1, float delay = 0, Action action = null)
        {
            if (target == null) yield break;
            yield return Fade(() => target.color, 
                c => target.color = c, start, end, fadeTime, delay, action);
        }

      

        #endregion

        
    }
}
