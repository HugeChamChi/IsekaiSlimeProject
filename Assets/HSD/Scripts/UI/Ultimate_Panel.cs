using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ultimate_Panel : MonoBehaviour
{
    [SerializeField] TMP_Text charName;
    [SerializeField] TMP_Text charNameEN;
    [SerializeField] Image charImage;
    private static readonly int inHash = Animator.StringToHash("In");
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void ShowUltimate()
    {
        gameObject.SetActive(true);
        anim.SetTrigger(inHash);
    }
}
