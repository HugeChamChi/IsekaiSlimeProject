using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Util;

public class BossAppearsPanel : MonoBehaviour
{
    [SerializeField] Image bossImage;
    [SerializeField] TMP_Text bossAppearsText;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Show(MonsterStat stat)
    {
        anim.SetTrigger(Utils.inHash);
        bossImage.sprite = stat.icon;
        bossAppearsText.text = stat.Name;
    }
}
