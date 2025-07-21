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

    private const float maxAlpha = 1.0f;
    private const float minAlpha = 110f / 255f;

    private Coroutine startRoutine;
    private YieldInstruction delay;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        delay = new WaitForSeconds(2);
    }

    public void Show()
    {
        if (warningRoutine != null) return;
        gameObject.SetActive(true);

        anim.SetTrigger(Utils.inHash);

        if(startRoutine == null)
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
        Debug.Log("1");
        yield return delay;
        Debug.Log("2");
        if (warningRoutine == null)
            warningRoutine = StartCoroutine(WarningRoutine());
    }

    private IEnumerator WarningRoutine()
    {        
        while(true)
        {
            timer = 0;
            tempColor = up.color;

            while (timer < time)
            {
                timer += Time.deltaTime;                
                tempColor.a = Mathf.Lerp(maxAlpha, minAlpha, timer / time);
                up.color = tempColor;
                down.color = tempColor;
                yield return null;
            }

            timer = 0;
            tempColor = up.color;

            while (timer < time)
            {
                timer += Time.deltaTime;
                tempColor.a = Mathf.Lerp(minAlpha, maxAlpha, timer / time);
                up.color = tempColor;
                down.color = tempColor;
                yield return null;
            }

            yield return null;
        }
    }

    private IEnumerator WarningStopRoutine()
    {
        timer = 0;
        tempColor = up.color;        

        while (timer < time)
        {
            timer += Time.deltaTime;
            tempColor.a = Mathf.Lerp(minAlpha, 0, timer / time);
            up.color = tempColor;
            down.color = tempColor;
            yield return null;
        }

        gameObject.SetActive(false);
        startRoutine = null;
        yield return null;
    }
}
