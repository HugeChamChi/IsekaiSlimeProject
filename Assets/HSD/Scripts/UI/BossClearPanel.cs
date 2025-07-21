using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Util;

public class BossClearPanel : MonoBehaviour
{
    [SerializeField] TMP_Text bossName;
    [SerializeField] TMP_Text goldAmount;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    [ContextMenu("Show")]
    public void Show(MonsterStat stat)
    {
        bossName.text = $"{stat.Name} 처치!";
        goldAmount.text = $"X {stat.DropGold.ToString()}";
        gameObject.SetActive(true);
        anim.SetTrigger(Utils.inHash);
    }
}
