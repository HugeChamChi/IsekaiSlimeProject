using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoController2 : MonoBehaviour
{
    [SerializeField] private VideoClip unit216;
    [SerializeField] private VideoClip unit217;
    [SerializeField] private GameObject videoPanel;
    
    
    private VideoPlayer videoPlayer;
    
    
    void Start()
    {
        // 재정의 , 오류감지용 겟컴퍼넌트
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.loopPointReached += (_) => videoPanel.SetActive(false);
    }

    public void PlayCutScene(int unitIndex)
    {
        videoPanel.gameObject.SetActive(true);
        videoPlayer.clip = unitIndex == 216 ? unit216 : unit217;
        videoPlayer.Play();
    }
    
    
}
