using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class WarningMessagePanel : MonoBehaviour
{
    private Animator anim;
    private Coroutine warningRoutine;

    [Header("WarningLine")]
    [SerializeField] Image up;
    [SerializeField] Image down;

    private const float time = .5f;
    private float timer;
    private Color tempColor;

    private const int max = 255;
    private const int min = 110;

    private Coroutine startRoutine;
    private YieldInstruction delay;

    private void Start()
    {
        delay = new WaitForSeconds(2);
    }

    public void Show()
    {
        if (warningRoutine != null) return;
        gameObject.SetActive(true);

        anim.SetTrigger(Utils.inHash);

        if(startRoutine != null)
        {
            StopCoroutine(startRoutine);
            startRoutine = null;
        }
        startRoutine = StartCoroutine(WarningStartRoutine());
    }

    public void StopWarning()
    {
        if (warningRoutine == null) return;
        StartCoroutine(WarningStopRoutine());
        StopCoroutine(warningRoutine);
        warningRoutine = null;
    }

    private IEnumerator WarningStartRoutine()
    {
        yield return delay;

        if (warningRoutine == null)
            warningRoutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {        
        while(true)
        {
            timer = 0;
            tempColor = up.color;

            while (timer > 0)
            {
                timer += Time.deltaTime;                
                tempColor.a = Mathf.Lerp(max, min, timer / time);
                up.color = tempColor;
                down.color = tempColor;
                return null;
            }

            timer = 0;
            tempColor = up.color;

            while (timer > 0)
            {
                timer += Time.deltaTime;
                tempColor.a = Mathf.Lerp(min, max, timer / time);
                up.color = tempColor;
                down.color = tempColor;
                return null;
            }

            return null;
        }
    }

    private IEnumerator WarningStopRoutine()
    {
        timer = 0;
        tempColor = up.color;        

        while (timer > 0)
        {
            timer += Time.deltaTime;
            tempColor.a = Mathf.Lerp(min, 0, timer / time);
            up.color = tempColor;
            down.color = tempColor;
            return null;
        }

        gameObject.SetActive(false);
        return null;
    }
}
