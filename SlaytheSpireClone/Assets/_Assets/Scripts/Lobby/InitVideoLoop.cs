using UnityEngine;
using UnityEngine.Video;
using DG.Tweening;

// 시작 화면 비디오 루프
public class InitVideoLoop : MonoBehaviour
{
    // 비디오 플레이어
    public VideoPlayer loopvideoPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (loopvideoPlayer.frame >= (long)(loopvideoPlayer.frameCount - 20))
        {
            GetComponent<CanvasGroup>().DOFade(0, 1).OnComplete(() =>
            {
                GetComponent<CanvasGroup>().DOFade(1, 0.5f);
            });
        }
    }
}
