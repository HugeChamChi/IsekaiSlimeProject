using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class CharacterInfoPanel : MonoBehaviour
{
    private Animator anim;
    [Header("DI_UI")]
    [SerializeField] Image characterImage;
    [SerializeField] TMP_Text characterName;
    [SerializeField] TMP_Text characterDescription;
    [SerializeField] TMP_Text skillDescription;


    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Show(Units.Unit info)
    {
        gameObject.SetActive(true);
        anim.ResetTrigger(Utils.outHash);
        characterImage.sprite = info.UnitSprite;
        characterName.text = info.Name;
        characterDescription.text = info.Description;
        skillDescription.text = info.Controller.Skill.Description;

        anim.SetTrigger(Utils.inHash);
    }

    public void Close()
    {        
        anim ??= GetComponent<Animator>();
        anim.ResetTrigger(Utils.inHash);
        anim.SetTrigger(Utils.outHash);
    }
}
