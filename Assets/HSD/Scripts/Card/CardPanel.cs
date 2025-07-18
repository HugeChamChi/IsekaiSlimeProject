using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPanel : MonoBehaviour
{
    [SerializeField] CardSlot[] slots;
    private Animator anim;
    private static readonly int inHash = Animator.StringToHash("In");
    private static readonly int outHash = Animator.StringToHash("Out");

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
        gameObject.SetActive(true);
        SetupCards();
        anim.SetTrigger(inHash);
    }   

    private void CloseCardPanel()
    {
        anim.ResetTrigger(inHash);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].isSelect = true;
        }
        anim.SetTrigger(outHash);
    }

    private void SetupCards()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetupData(Manager.Card.GetRandomCardData());
        }
    }
}
