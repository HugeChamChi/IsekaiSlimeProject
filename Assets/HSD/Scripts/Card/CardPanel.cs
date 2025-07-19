using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class CardPanel : MonoBehaviour
{
    [SerializeField] CardSlot[] slots;
    private Animator anim;    

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].OnSelected += CloseCardPanel;
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].OnSelected -= CloseCardPanel;
        }
    }

    [ContextMenu("Play")]
    public void OpenCardPanel()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].isSelect = false;
        }
        gameObject.SetActive(true);
        SetupCards();
        anim.SetTrigger(Utils.inHash);
    }   

    private void CloseCardPanel()
    {
        anim.ResetTrigger(Utils.inHash);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].isSelect = true;
        }
        anim.SetTrigger(Utils.outHash);
    }

    private void SetupCards()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetupData(Manager.Card.GetRandomCardData());
        }
    }
}
