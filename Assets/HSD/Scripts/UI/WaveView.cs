using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveView : MonoBehaviour
{
    [SerializeField] TMP_Text waveText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text monsterCountText;

    public void UpdateWaveText(int amount)
    {
        waveText.text = $"{amount+1}".ToString();
    }

    public void UpdateTimerText(float time)
    {
        timerText.text = time.ToString("F2");
    }

    public void UpdateMonsterCountText(int amount)
    {
        monsterCountText.text = $"{amount.ToString()} / 100";        
    }
}
