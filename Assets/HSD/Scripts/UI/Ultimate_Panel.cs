using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class Ultimate_Panel : MonoBehaviour
{
    [SerializeField] TMP_Text charName;
    [SerializeField] TMP_Text charNameEN;
    [SerializeField] Image charImage;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void ShowUltimate()
    {
        gameObject.SetActive(true);
        anim.SetTrigger(Utils.inHash);
    }
}
