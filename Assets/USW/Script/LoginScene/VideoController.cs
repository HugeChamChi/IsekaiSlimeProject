using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class VideoController : MonoBehaviour
{
   private VideoPlayer videoPlayer;
    void Start()
    {
        // 재정의 , 오류감지용 겟컴퍼넌트
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            return;
        }
    }
}
