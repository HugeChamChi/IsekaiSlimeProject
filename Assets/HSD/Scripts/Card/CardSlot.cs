using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{
    private CardData data;
    private Button slot;

    [Header("UI")]
    [SerializeField] TMP_Text cardName;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text cardDescription;

    public bool isSelect;
    public event Action OnSelected;

    private void Awake()
    {
        slot = GetComponent<Button>();       
    }

    private void OnEnable()
    {
        slot.onClick.AddListener(SelectCard);
    }

    private void OnDisable()
    {
        slot.onClick.RemoveListener(SelectCard);
    }

    public void SetupData(CardData _data)
    {
        if (_data == null) return;
        data = _data;

        cardName.text = data.cardName;
        icon.sprite = data.icon;
        cardDescription.text = data.description;
    }

    private void SelectCard()
    {
        if (isSelect) return;

        if (data != null)
            data.effect.Execute();

        OnSelected?.Invoke();
    }
}